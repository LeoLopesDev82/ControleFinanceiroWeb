namespace ControleFinanceiroWeb.Models.Entities
{
    public class Statement
    {
        public int Id { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? Amount { get; set; }
        public string? Description { get; set; }
        public int? EntryId { get; set; }
        public int? StatementTypeId { get; set; }
    }
}
