using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiroWeb.Models.ViewModels
{
    public class PinSetupViewModel
    {
        [Required(ErrorMessage = "Informe o PIN.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "O PIN deve ter exatamente 6 dígitos.")]
        public string Pin { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme o PIN.")]
        [Compare(nameof(Pin), ErrorMessage = "A confirmação não confere com o PIN.")]
        public string ConfirmPin { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
    }
}
