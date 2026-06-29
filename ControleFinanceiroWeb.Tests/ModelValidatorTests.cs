using ControleFinanceiroWeb.Helpers;
using ControleFinanceiroWeb.Models.ViewModels;

namespace ControleFinanceiroWeb.Tests
{
    public class ModelValidatorTests
    {
        [Fact]
        public void Validate_ShouldReturnSuccess_WhenModelIsValid()
        {
            var model = new CategoryFormViewModel
            {
                Id = "0",
                Description = "Alimentação",
                EntryType = "V",
                StatementIdentifiers = "MERCADO|RESTAURANTE"
            };

            var result = ModelValidator.Validate(model);

            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnFail_WhenRequiredFieldIsMissing()
        {
            var model = new CategoryFormViewModel
            {
                Id = "0",
                Description = null,
                EntryType = "V"
            };

            var result = ModelValidator.Validate(model);

            Assert.False(result.Success);
            Assert.NotNull(result.Message);
            Assert.Contains("A descrição da categoria é obrigatória.", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnFail_WhenFieldExceedsMaxLength()
        {
            var model = new CategoryFormViewModel
            {
                Id = "0",
                Description = new string('A', 260), 
                EntryType = "V"
            };

            var result = ModelValidator.Validate(model);

            Assert.False(result.Success);
            Assert.NotNull(result.Message);
            Assert.Contains("A descrição não pode ter mais de 255 caracteres.", result.Message);
        }
    }
}