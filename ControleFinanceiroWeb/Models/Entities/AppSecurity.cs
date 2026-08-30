using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControleFinanceiroWeb.Models.Entities
{
    [Table("APP_SECURITY")]
    public class AppSecurity
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("PIN_HASH")]
        [MaxLength(255)]
        public string PinHash { get; set; } = string.Empty;

        [Required]
        [Column("SECURITY_STAMP")]
        [MaxLength(64)]
        public string SecurityStamp { get; set; } = string.Empty;
    }
}
