using ControleFinanceiroDesktop.Models.ViewModels;
using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Models.Domain;
using ControleFinanceiroWeb.Models.DTOs;
using ControleFinanceiroWeb.Services.CategoryServices;

namespace ControleFinanceiroDesktop.Services.Header
{
    public class MainViewServices
    {
        private readonly AppDbContext _context;

        private readonly CategoryListServices _categoryListServices;

        public MainViewServices(AppDbContext context, CategoryListServices categoryListServices)
        {
            _context = context;
            _categoryListServices = categoryListServices;
        }

        public MainModel GetMainFormModel(DateTime startDate, DateTime endDate, int id)
		{
			MainModel model = new MainModel();

			model.ButtonsInfo = GetButtonInfoList();
			model.statementDtos = GetStatementGridList(startDate, endDate, id);
			model.SummaryDto = GetSummaryDto(startDate, endDate, id);
			model.CreditSummaries = GetCreditSummaries(startDate, endDate, id);
			model.DebitSummaries = GetDebitSummaries(startDate, endDate, id);
            model.Title = GetTitle(id, model.ButtonsInfo);
            model.Categories = id > -1 ? _categoryListServices.GetList() : new List<ListItemDto>();
            
            return model;
		}

        #region "subroutines"		

        private string GetTitle(int id, List<ButtonInfoModel> buttonsInfo)
        {
            if (id == -1)  return "Resumo";
            if (id == 0) return "Extrato";

            var button = buttonsInfo.FirstOrDefault(b => b.Id == id);

            return button?.Description ?? string.Empty;
        }

        private List<ButtonInfoModel> GetButtonInfoList()
        {
            return _context.StatementTypes
                .Select(st => new ButtonInfoModel
                {
                    Id = st.Id,
                    Description = st.Description ?? string.Empty
                })
                .ToList();
        }

        private List<StatementGridDto> GetStatementGridList(DateTime startDate, DateTime endDate, int id)
        {
            if (id == -1) return new List<StatementGridDto>();

            var query = from s in _context.Statement
                        where s.TransactionDate >= startDate &&
                              s.TransactionDate <= endDate &&
                              s.StatementTypeId == id
                        join c in _context.Categories
                            on s.EntryId equals c.Id into gj
                        from category in gj.DefaultIfEmpty()
                        orderby s.TransactionDate
                        select new StatementGridDto
                        {
                            Id = s.Id,
                            TransactionDate = s.TransactionDate,
                            DueDate = s.DueDate,
                            Amount = s.Amount,
                            Description = s.Description,
                            EntryId = s.EntryId,
                            EntryDescription = category != null ? category.Description : null
                        };

            return query.ToList();
        }

        private SummaryDto GetSummaryDto(DateTime startDate, DateTime endDate, int id)
        {
            SummaryDto summary = new SummaryDto();

            if (id > -1) return summary;

            var statements = _context.Statement.Where(s => s.DueDate >= startDate && s.DueDate <= endDate).ToList();

            summary.Credits = statements.Where(s => s.Amount > 0).Sum(s => s.Amount);
            summary.Debits = statements.Where(s => s.Amount < 0).Sum(s => Math.Abs(s.Amount ?? 0));
            summary.Balance = summary.Credits - summary.Debits;

            return summary;
        }

        private List<CategorySummaryDto> GetCreditSummaries(DateTime startDate, DateTime endDate, int statementTypeId)
        {
            return GetCategorySummariesInternal(startDate, endDate, statementTypeId, isCredit: true);
        }

        private List<CategorySummaryDto> GetDebitSummaries(DateTime startDate, DateTime endDate, int statementTypeId)
        {
            return GetCategorySummariesInternal(startDate, endDate, statementTypeId, isCredit: false);
        }

        private List<CategorySummaryDto> GetCategorySummariesInternal(DateTime startDate, DateTime endDate, int statementTypeId, bool isCredit)
        {
            if (statementTypeId > -1) return new List<CategorySummaryDto>();

            var rawData = (
                from s in _context.Statement
                where s.DueDate >= startDate
                   && s.DueDate <= endDate
                   && ((isCredit && s.Amount > 0) || (!isCredit && s.Amount < 0))
                join c in _context.Categories
                    on s.EntryId equals (int?)c.Id into sc
                from c in sc.DefaultIfEmpty()
                select new
                {
                    Amount = isCredit ? (s.Amount ?? 0) : Math.Abs(s.Amount ?? 0),
                    Category = c,
                }
            ).ToList()
            .Select(x => new
            {
                x.Amount,
                CategoryName = x.Category != null
                    ? x.Category.Description + " (" + (x.Category.EntryType == 'F' ? "Fixo" : "Variável") + ")"
                    : "Não definida (Variável)"
            });

            var grouped = rawData
                .GroupBy(x => x.CategoryName)
                .Select(g => new CategorySummaryDto
                {
                    Category = g.Key,
                    Amount = g.Sum(x => x.Amount)
                })
                .ToList();

            decimal totalAmount = grouped.Sum(x => x.Amount);

            var result = new List<CategorySummaryDto>
            {
                new CategorySummaryDto
                {
                    Category = "Total",
                    Amount = totalAmount,
                    Percentage = 100
                }
            };

            var ordered = grouped
                .Select(g => new CategorySummaryDto
                {
                    Category = g.Category,
                    Amount = g.Amount,
                    Percentage = totalAmount > 0 ? Math.Round((g.Amount / totalAmount) * 100, 2) : 0
                })
                .OrderByDescending(x => x.Percentage)
                .ToList();

            result.AddRange(ordered);

            return result;
        }

        #endregion

    }
}