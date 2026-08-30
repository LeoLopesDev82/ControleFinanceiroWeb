using ControleFinanceiroWeb.Models;
namespace ControleFinanceiroWeb.Models.ViewModels
{
    public class TransactionsViewModel
    {
        public DateRange Period { get; set; }

        public string StatementTypeName { get; set; } = "Extrato";

        public int StatementTypeId { get; set; }

        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal FinalBalance { get; set; }
        public List<TransactionGridItemViewModel> GridItems { get; set; } = new();
    }

    public class TransactionGridItemViewModel
    {
        public int Id { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}