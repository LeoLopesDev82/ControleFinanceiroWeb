namespace ControleFinanceiroWeb.Models.DTOs
{
    public class StatementGridDto
    {
        public int Id { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? Amount { get; set; }
        public string? Description { get; set; }
        public int? EntryId { get; set; }
        public string? EntryDescription { get; set; }
    }
}
