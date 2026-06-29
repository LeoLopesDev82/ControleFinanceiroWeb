namespace ControleFinanceiroWeb.Models.ViewModels
{
    public class TransactionsViewModel
    {
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