using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControleFinanceiroWeb.Models.Entities
{
    [Table("STATEMENT")]
    public class Statement
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("TRANSACTION_DATE")]
        public DateTime? TransactionDate { get; set; }

        [Column("DUE_DATE")]
        public DateTime? DueDate { get; set; }

        [Column("AMOUNT", TypeName = "numeric(18,2)")]
        public decimal? Amount { get; set; }

        [Column("DESCRIPTION")]
        [MaxLength(255)]
        public string? Description { get; set; }

        [Column("ENTRY_ID")]
        public int? CategoryId { get; set; }

        [Column("STATEMENT_TYPE_ID")]
        public int? StatementTypeId { get; set; }
    }
}