using System;
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

        [Column("FAILED_ATTEMPTS")]
        public int FailedAttempts { get; set; }

        [Column("LOCKED_UNTIL")]
        public DateTime? LockedUntil { get; set; }
    }
}
