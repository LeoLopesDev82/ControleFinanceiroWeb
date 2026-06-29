using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiroWeb.Models.ViewModels
{
    public class StatementTypeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A descrição não pode ser nula!")]
        [MaxLength(255, ErrorMessage = "A descrição não pode conter mais do que 255 caracteres")]
        public string Name { get; set; } = string.Empty;
    }
}