using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiroWeb.Models.ViewModels
{
    public class PinChangeViewModel
    {
        [Required(ErrorMessage = "Informe o PIN atual.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "O PIN deve ter exatamente 6 dígitos.")]
        public string CurrentPin { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o novo PIN.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "O PIN deve ter exatamente 6 dígitos.")]
        public string NewPin { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme o novo PIN.")]
        [Compare(nameof(NewPin), ErrorMessage = "A confirmação não confere com o novo PIN.")]
        public string ConfirmPin { get; set; } = string.Empty;
    }
}
