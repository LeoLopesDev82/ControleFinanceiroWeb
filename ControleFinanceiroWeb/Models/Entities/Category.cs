using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControleFinanceiroWeb.Models.Entities
{
    [Table("CATEGORY")]
    public class Category
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("DESCRIPTION")]
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column("ENTRY_TYPE")]
        public CategoryType EntryType { get; set; }

        [Required]
        [Column("STATEMENT_IDENTIFIERS")]
        [MaxLength(5000)]
        public string StatementIdentifiers { get; set; } = string.Empty;
    }
}