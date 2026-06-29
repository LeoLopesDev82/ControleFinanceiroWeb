namespace ControleFinanceiroWeb.Models.ViewModels
{
    public class TransactionImportPreviewItemViewModel
    {
        public int RowIndex { get; set; }
        public string? RawDate { get; set; }
        public string? ParsedDate { get; set; }
        public string? RawDueDate { get; set; }
        public string? ParsedDueDate { get; set; }
        public string? Description { get; set; }
        public string? RawAmount { get; set; }
        public decimal? ParsedAmount { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }
}