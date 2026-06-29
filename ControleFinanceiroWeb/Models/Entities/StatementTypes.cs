using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControleFinanceiroWeb.Models.Entities
{
    [Table("STATEMENT_TYPES")]
    public class StatementTypes
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("DESCRIPTION")]
        [MaxLength(255)]
        public string? Description { get; set; }
    }
}