using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Models;
using ControleFinanceiroWeb.Models.Entities;
using ControleFinanceiroWeb.Models.ViewModels;
using ControleFinanceiroWeb.Services.Categories;

namespace ControleFinanceiroWeb.Services.Transactions
{
    // Service responsible for handling CRUD operations, lists, and Excel import batch parsing for transactions.
    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TransactionService> _logger;
        private readonly ICategoryIdentificationService _categoryIdentificationService;

        // Initializes a new instance of TransactionService.
        public TransactionService(AppDbContext context, ICategoryIdentificationService categoryIdentificationService, ILogger<TransactionService> logger)
        {
            _context = context;
            _categoryIdentificationService = categoryIdentificationService;
            _logger = logger;
        }

        #region Public Methods

        // Fetches all transactions filtered by account and date range, calculating financial balances.
        public async Task<TransactionsViewModel> GetTransactionsAsync(int statementTypeId, DateTime startDate, DateTime endDate)
        {
            var rawQuery = from s in _context.Statement
                           where s.StatementTypeId == statementTypeId &&
                                 s.TransactionDate >= startDate &&
                                 s.TransactionDate <= endDate
                           join c in _context.Categories
                               on s.CategoryId equals c.Id into catGroup
                           from category in catGroup.DefaultIfEmpty()
                           orderby s.TransactionDate
                           select new
                           {
                               s.Id,
                               s.TransactionDate,
                               s.DueDate,
                               s.Description,
                               s.Amount,
                               CategoryDescription = category.Description
                           };

            var rawList = await rawQuery.ToListAsync();

            var gridItems = rawList.Select(x => new TransactionGridItemViewModel
            {
                Id = x.Id,
                TransactionDate = x.TransactionDate,
                DueDate = x.DueDate,
                Description = x.Description ?? string.Empty,
                CategoryName = x.CategoryDescription ?? "Não definida",
                Amount = x.Amount ?? 0
            }).ToList();

            decimal totalCredits = gridItems.Where(g => g.Amount > 0).Sum(g => g.Amount);
            decimal totalDebits = gridItems.Where(g => g.Amount < 0).Sum(g => Math.Abs(g.Amount));
            decimal finalBalance = totalCredits - totalDebits;

            return new TransactionsViewModel
            {
                TotalCredits = totalCredits,
                TotalDebits = totalDebits,
                FinalBalance = finalBalance,
                GridItems = gridItems
            };
        }

        // Fetches list of categories formatted as dropdown option view models.
        public async Task<List<CategoryOptionViewModel>> GetCategoriesAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Description)
                .Select(c => new CategoryOptionViewModel
                {
                    Id = c.Id,
                    Description = c.Description,
                    EntryType = (char)c.EntryType
                })
                .ToListAsync();
        }

        // Generates the transaction form view model, loading existing data or building default insertion states.
        public async Task<TransactionFormViewModel> GetTransactionFormAsync(int id, int statementTypeId)
        {
            var model = await GetOrCreateTransactionFormModelAsync(id, statementTypeId);

            await PopulateCategoryOptionsAsync(model);

            return model;
        }

        // Validates and saves (inserts or updates) a transaction entity.
        public async Task<ServiceResult> SaveTransactionAsync(TransactionFormViewModel model)
        {
            var validationResult = ValidateTransactionForm(model);

            if (!validationResult.Success)
                return validationResult;

            int id = Helpers.ConversionHelper.ToNullableInt(model.Id) ?? 0;

            try
            {
                if (id == 0)
                    return await InsertTransactionAsync(model);
                else
                    return await UpdateTransactionAsync(model, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save a statement.");

                return new ServiceResult
                {
                    Success = false,
                    Message = "Ocorreu um erro inesperado ao salvar os dados no banco de dados."
                };
            }
        }

        // Deletes a transaction entity by id.
        public async Task<ServiceResult> DeleteTransactionAsync(int id)
        {
            try
            {
                var existing = await _context.Statement.FirstOrDefaultAsync(s => s.Id == id);

                if (existing == null)
                {
                    return new ServiceResult { Success = false, Message = "• Registro não encontrado para exclusão." };
                }

                _context.Statement.Remove(existing);
                await _context.SaveChangesAsync();

                return new ServiceResult { Success = true, Message = "Movimentação excluída com sucesso." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete statement.");

                return new ServiceResult { Success = false, Message = "Ocorreu um erro ao excluir a movimentação." };
            }
        }

        // Parses raw clipboard data into a structured batch import preview list.
        public async Task<List<TransactionImportPreviewItemViewModel>> PreviewImportAsync(string rawText)
        {
            var resultList = new List<TransactionImportPreviewItemViewModel>();

            if (string.IsNullOrWhiteSpace(rawText))
                return resultList;

            var categories = await _context.Categories
                .Where(c => !string.IsNullOrEmpty(c.StatementIdentifiers))
                .ToListAsync();

            var categoryMap = await _context.Categories.ToDictionaryAsync(c => c.Id, c => c.Description);
            var lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int rowIndex = 0;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                rowIndex++;

                var item = ParseImportLine(line, rowIndex, categories, categoryMap);

                resultList.Add(item);
            }

            return resultList;
        }

        // Saves a batch list of imported statements in a single transaction.
        public async Task<ServiceResult> SaveImportAsync(List<TransactionImportSaveModel> items, int statementTypeId)
        {
            if (items == null || !items.Any())
            {
                return new ServiceResult { Success = false, Message = "Nenhum item válido para importação." };
            }

            try
            {
                var newStatements = new List<Statement>();

                foreach (var item in items)
                {
                    if (!ParseImportSaveModel(item, statementTypeId, out var statement))
                    {
                        return new ServiceResult { Success = false, Message = "Dados corrompidos ou inválidos durante o salvamento da importação." };
                    }

                    if (statement != null)
                    {
                        newStatements.Add(statement);
                    }
                }

                _context.Statement.AddRange(newStatements);
                await _context.SaveChangesAsync();

                return new ServiceResult
                {
                    Success = true,
                    Message = newStatements.Count == 1
                        ? "1 lançamento foi importado com sucesso!"
                        : $"{newStatements.Count} lançamentos foram importados com sucesso!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save the imported statements.");

                return new ServiceResult { Success = false, Message = "Ocorreu um erro ao gravar a importação no banco de dados." };
            }
        }

        #endregion

        #region Private Methods

        // Validates the presence and format constraints of the transaction form model.
        private ServiceResult ValidateTransactionForm(TransactionFormViewModel model)
        {
            var validate = Helpers.ModelValidator.Validate(model);

            if (!validate.Success)
                return validate;

            var sb = new System.Text.StringBuilder();

            var parsedAmount = Helpers.ConversionHelper.ToNullableDecimal(model.Amount);
            var parsedTransactionDate = Helpers.ConversionHelper.ToNullableDateTime(model.TransactionDate);
            var parsedDueDate = Helpers.ConversionHelper.ToNullableDateTime(model.DueDate);

            if (!parsedAmount.HasValue)
                sb.AppendLine("• O valor informado é inválido. Digite um número válido.");

            if (!parsedTransactionDate.HasValue)
                sb.AppendLine("• A data de movimentação é obrigatória ou inválida.");

            if (!parsedDueDate.HasValue)
                sb.AppendLine("• A data de vencimento é obrigatória ou inválida.");

            if (sb.Length > 0)
            {
                return new ServiceResult { Success = false, Message = sb.ToString().TrimEnd() };
            }

            return new ServiceResult { Success = true };
        }

        // Fetches an existing transaction for form binding or initializes a new model.
        private async Task<TransactionFormViewModel> GetOrCreateTransactionFormModelAsync(int id, int statementTypeId)
        {
            if (id > 0)
            {
                var s = await _context.Statement.FirstOrDefaultAsync(st => st.Id == id);

                if (s != null)
                {
                    return new TransactionFormViewModel
                    {
                        Id = s.Id.ToString(),
                        TransactionDate = s.TransactionDate?.ToString("yyyy-MM-dd"),
                        DueDate = s.DueDate?.ToString("yyyy-MM-dd"),
                        Amount = s.Amount?.ToString("F2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")) ?? "0,00",
                        Description = s.Description,
                        CategoryId = s.CategoryId?.ToString(),
                        StatementTypeId = s.StatementTypeId?.ToString()
                    };
                }
            }

            return new TransactionFormViewModel
            {
                Id = "0",
                StatementTypeId = statementTypeId.ToString(),
                TransactionDate = DateTime.Today.ToString("yyyy-MM-dd")
            };
        }

        // Populates categories option list inside the transaction form view model.
        private async Task PopulateCategoryOptionsAsync(TransactionFormViewModel model)
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Description)
                .ToListAsync();

            model.CategoryOptions.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = "",
                Text = "-- Selecione a Categoria --"
            });

            foreach (var cat in categories)
            {
                string typeLabel = cat.EntryType == CategoryType.Fixed ? "Fixo" : "Variável";

                model.CategoryOptions.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = cat.Id.ToString(),
                    Text = $"{cat.Description} [{typeLabel}]"
                });
            }
        }

        // Maps properties from the view model to a transaction database entity.
        private void MapViewModelToEntity(TransactionFormViewModel model, Statement entity)
        {
            entity.TransactionDate = Helpers.ConversionHelper.ToNullableDateTime(model.TransactionDate);
            entity.DueDate = Helpers.ConversionHelper.ToNullableDateTime(model.DueDate);
            entity.Amount = Helpers.ConversionHelper.ToNullableDecimal(model.Amount);
            entity.Description = Helpers.ConversionHelper.ToNullableString(model.Description);
            entity.CategoryId = Helpers.ConversionHelper.ToNullableInt(model.CategoryId);
            entity.StatementTypeId = Helpers.ConversionHelper.ToNullableInt(model.StatementTypeId);
        }

        // Inserts a new transaction record into the database.
        private async Task<ServiceResult> InsertTransactionAsync(TransactionFormViewModel model)
        {
            var newStatement = new Statement();

            MapViewModelToEntity(model, newStatement);

            _context.Statement.Add(newStatement);
            await _context.SaveChangesAsync();

            return new ServiceResult
            {
                Success = true,
                Message = "Movimentação incluída com sucesso!",
                Id = newStatement.Id
            };
        }

        // Updates an existing transaction record in the database.
        private async Task<ServiceResult> UpdateTransactionAsync(TransactionFormViewModel model, int id)
        {
            var existing = await _context.Statement.FirstOrDefaultAsync(s => s.Id == id);

            if (existing == null)
            {
                return new ServiceResult { Success = false, Message = "• Registro não encontrado para edição." };
            }

            MapViewModelToEntity(model, existing);

            await _context.SaveChangesAsync();

            return new ServiceResult
            {
                Success = true,
                Message = "Movimentação atualizada com sucesso!",
                Id = existing.Id
            };
        }

        // Parses and validates a single raw tab-separated line into a preview model.
        private TransactionImportPreviewItemViewModel ParseImportLine(
            string line,
            int rowIndex,
            List<Category> categories,
            Dictionary<int, string> categoryMap)
        {
            var cols = line.Split('\t');

            string rawDate = cols.Length > 0 ? cols[0].Trim() : string.Empty;
            string rawDueDate = cols.Length > 1 ? cols[1].Trim() : string.Empty;
            string rawDesc = cols.Length > 2 ? cols[2].Trim() : string.Empty;
            string rawAmount = cols.Length > 3 ? cols[3].Trim() : string.Empty;

            var item = new TransactionImportPreviewItemViewModel
            {
                RowIndex = rowIndex,
                RawDate = rawDate,
                RawDueDate = rawDueDate,
                Description = rawDesc,
                RawAmount = rawAmount,
                IsValid = true
            };

            if (cols.Length < 4)
            {
                item.IsValid = false;
                item.ErrorMessage = "• Linha incompleta. Esperado 4 colunas (Data Mov., Data Venc., Descrição, Valor).";

                return item;
            }

            ValidateAndParseImportItem(item, rawDate, rawDueDate, rawDesc, rawAmount);

            if (item.IsValid)
            {
                IdentifyAndSetImportCategory(item, rawDesc, categories, categoryMap);
            }

            return item;
        }

        // Checks and parses cells format (dates, description, decimal values) for the import line.
        private void ValidateAndParseImportItem(
            TransactionImportPreviewItemViewModel item,
            string rawDate,
            string rawDueDate,
            string rawDesc,
            string rawAmount)
        {
            DateTime? parsedDate = Helpers.ConversionHelper.ToNullableDateTime(rawDate);
            DateTime? parsedDueDate = Helpers.ConversionHelper.ToNullableDateTime(rawDueDate);

            decimal? parsedAmount = Helpers.ConversionHelper.ToNullableDecimal(rawAmount);

            if (string.IsNullOrWhiteSpace(rawDate))
            {
                item.IsValid = false;
                item.ErrorMessage += "• A data de movimentação é obrigatória. ";
            }
            else if (!parsedDate.HasValue)
            {
                item.IsValid = false;
                item.ErrorMessage += $"• Data de movimentação inválida ('{rawDate}'). ";
            }
            else
            {
                item.ParsedDate = parsedDate.Value.ToString("yyyy-MM-dd");
            }

            if (string.IsNullOrWhiteSpace(rawDueDate))
            {
                item.IsValid = false;
                item.ErrorMessage += "• A data de vencimento é obrigatória. ";
            }
            else if (!parsedDueDate.HasValue)
            {
                item.IsValid = false;
                item.ErrorMessage += $"• Data de vencimento inválida ('{rawDueDate}'). ";
            }
            else
            {
                item.ParsedDueDate = parsedDueDate.Value.ToString("yyyy-MM-dd");
            }

            if (string.IsNullOrWhiteSpace(rawDesc))
            {
                item.IsValid = false;
                item.ErrorMessage += "• A descrição/histórico é obrigatória. ";
            }

            if (string.IsNullOrWhiteSpace(rawAmount))
            {
                item.IsValid = false;
                item.ErrorMessage += "• O valor é obrigatório. ";
            }
            else if (!parsedAmount.HasValue)
            {
                item.IsValid = false;
                item.ErrorMessage += $"• Valor inválido ('{rawAmount}'). ";
            }
            else
            {
                item.ParsedAmount = parsedAmount.Value;
            }
        }

        // Matches the description of the import item against active category keywords and sets it.
        private void IdentifyAndSetImportCategory(
            TransactionImportPreviewItemViewModel item,
            string rawDesc,
            List<Category> categories,
            Dictionary<int, string> categoryMap)
        {
            int? matchedCategoryId = _categoryIdentificationService.IdentifyCategory(rawDesc, categories);

            if (matchedCategoryId.HasValue)
            {
                item.CategoryId = matchedCategoryId.Value;

                if (categoryMap.TryGetValue(matchedCategoryId.Value, out var catName))
                {
                    item.CategoryName = catName;
                }
                else
                {
                    item.CategoryName = "Não definida";
                }
            }
            else
            {
                item.CategoryName = "Não definida";
            }
        }

        // Validates and maps a single imported transaction item into a database entity.
        private bool ParseImportSaveModel(TransactionImportSaveModel item, int statementTypeId, out Statement? statement)
        {
            statement = null;

            decimal? amount = Helpers.ConversionHelper.ToNullableDecimal(item.Amount);
            
            DateTime? transactionDate = Helpers.ConversionHelper.ToNullableDateTime(item.TransactionDate);
            DateTime? dueDate = Helpers.ConversionHelper.ToNullableDateTime(item.DueDate);

            if (!amount.HasValue || !transactionDate.HasValue || !dueDate.HasValue || string.IsNullOrWhiteSpace(item.Description))
            {
                return false;
            }

            statement = new Statement
            {
                TransactionDate = transactionDate.Value,
                DueDate = dueDate.Value,
                Amount = amount.Value,
                Description = Helpers.ConversionHelper.ToNullableString(item.Description),
                CategoryId = item.CategoryId > 0 ? item.CategoryId : null,
                StatementTypeId = statementTypeId
            };

            return true;
        }

        #endregion
    }
}