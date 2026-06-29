using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Models;
using ControleFinanceiroWeb.Models.Entities;

namespace ControleFinanceiroWeb.Services.Categories
{
    public class CategoryIdentificationService : ICategoryIdentificationService
    {
        private readonly AppDbContext _context;

        // Initializes a new instance of CategoryIdentificationService.
        public CategoryIdentificationService(AppDbContext context)
        {
            _context = context;
        }

        #region Public Methods

        // Matches a transaction description against a list of category keywords.
        // Splits indicators by pipe '|' and performs case-insensitive searches.
        public int? IdentifyCategory(string? description, IEnumerable<Category> categories)
        {
            if (string.IsNullOrWhiteSpace(description))
                return null;

            foreach (var category in categories)
            {
                if (string.IsNullOrWhiteSpace(category.StatementIdentifiers))
                    continue;

                var keywords = category.StatementIdentifiers.Split('|', StringSplitOptions.RemoveEmptyEntries);

                foreach (var keyword in keywords)
                {
                    var trimmed = keyword.Trim();

                    if (string.IsNullOrWhiteSpace(trimmed))
                        continue;

                    if (description.Contains(trimmed, StringComparison.OrdinalIgnoreCase))
                        return category.Id;
                }
            }

            return null;
        }

        // Gathers pending transactions in a period and triggers automatic category matching.
        public async Task<ServiceResult> IdentifyTransactionsAsync(int statementTypeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var categories = await GetCategoriesWithIdentifiersAsync();
                var transactions = await GetPendingTransactionsAsync(statementTypeId, startDate, endDate);

                if (!transactions.Any())
                {
                    return new ServiceResult
                    {
                        Success = true,
                        Message = "Nenhuma movimentação pendente de categorização no período selecionado."
                    };
                }

                int identifiedCount = ProcessTransactionsCategorization(transactions, categories);

                if (identifiedCount > 0) 
                    await _context.SaveChangesAsync();

                return new ServiceResult
                {
                    Success = true,
                    Message = BuildSuccessResultMessage(identifiedCount)
                };
            }
            catch (Exception)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Ocorreu um erro inesperado ao processar a identificação de categorias."
                };
            }
        }

        #endregion

        #region Private Methods

        // Fetches all categories that have statement identifier keywords configured.
        private async Task<List<Category>> GetCategoriesWithIdentifiersAsync()
        {
            return await _context.Categories
                .Where(c => !string.IsNullOrEmpty(c.StatementIdentifiers))
                .ToListAsync();
        }

        // Fetches pending transactions (without a category assigned) in a given period.
        private async Task<List<Statement>> GetPendingTransactionsAsync(int statementTypeId, DateTime startDate, DateTime endDate)
        {
            return await _context.Statement
                .Where(s => s.StatementTypeId == statementTypeId &&
                            s.TransactionDate >= startDate &&
                            s.TransactionDate <= endDate &&
                            s.EntryId == null)
                .ToListAsync();
        }

        // Matches and assigns categories to list of statements, returning count of identified transactions.
        private int ProcessTransactionsCategorization(List<Statement> transactions, List<Category> categories)
        {
            int identifiedCount = 0;

            foreach (var t in transactions)
            {
                int? categoryId = IdentifyCategory(t.Description, categories);

                if (categoryId.HasValue)
                {
                    t.EntryId = categoryId.Value;

                    identifiedCount++;
                }
            }

            return identifiedCount;
        }

        // Builds dynamic feedback message depending on how many transactions were identified.
        private string BuildSuccessResultMessage(int identifiedCount)
        {
            if (identifiedCount > 0)
            {
                return identifiedCount == 1
                    ? "1 movimentação foi identificada e categorizada automaticamente!"
                    : $"{identifiedCount} movimentações foram identificadas e categorizadas automaticamente!";
            }

            return "A análise foi concluída, mas nenhuma movimentação pendente pôde ser identificada com as palavras-chave cadastradas nas categorias.";
        }

        #endregion
    }
}