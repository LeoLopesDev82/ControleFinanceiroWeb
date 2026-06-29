using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiroWeb.Models.ViewModels
{
    public class CategoryFormViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "A descrição da categoria é obrigatória.")]
        [MaxLength(255, ErrorMessage = "A descrição não pode ter mais de 255 caracteres.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "O tipo de lançamento é obrigatório.")]
        public string? EntryType { get; set; }

        [MaxLength(5000, ErrorMessage = "Os identificadores de extrato não podem ter mais de 5000 caracteres.")]
        public string? StatementIdentifiers { get; set; }
    }
}