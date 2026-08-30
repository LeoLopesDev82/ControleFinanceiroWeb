using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiroWeb.Models.ViewModels
{
    public class PinLoginViewModel
    {
        [Required(ErrorMessage = "Informe o PIN.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "O PIN deve ter exatamente 6 dígitos.")]
        public string Pin { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
