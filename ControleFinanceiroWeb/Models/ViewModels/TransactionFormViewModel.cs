using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleFinanceiroWeb.Models.ViewModels
{
    public class TransactionFormViewModel
    {
        public List<SelectListItem> CategoryOptions { get; set; } = new();
        public string? Id { get; set; }

        [Required(ErrorMessage = "A data de movimentação é obrigatória.")]
        public string? TransactionDate { get; set; }

        [Required(ErrorMessage = "A data de vencimento é obrigatória.")]
        public string? DueDate { get; set; }

        [Required(ErrorMessage = "O valor da movimentação é obrigatório.")]
        public string? Amount { get; set; }

        [Required(ErrorMessage = "O histórico/descrição é obrigatório.")]
        [MaxLength(255, ErrorMessage = "O histórico/descrição não pode conter mais de 255 caracteres.")]
        public string? Description { get; set; }

        public string? CategoryId { get; set; }

        [Required(ErrorMessage = "O tipo de extrato é inválido.")]
        public string? StatementTypeId { get; set; }
    }
}