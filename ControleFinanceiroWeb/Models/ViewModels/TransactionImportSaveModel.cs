namespace ControleFinanceiroWeb.Models.ViewModels
{
    public class TransactionImportSaveModel
    {
        public string? TransactionDate { get; set; }
        public string? DueDate { get; set; }
        public string? Amount { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
    }
}