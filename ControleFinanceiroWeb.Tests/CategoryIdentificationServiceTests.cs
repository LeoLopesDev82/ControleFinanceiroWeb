using ControleFinanceiroWeb.Models.Entities;
using ControleFinanceiroWeb.Services.Categories;

namespace ControleFinanceiroWeb.Tests
{
    public class CategoryIdentificationServiceTests
    {
        private List<Category> GetMockCategories()
        {
            return new List<Category>
            {
                new Category 
                { 
                    Id = 10, 
                    Description = "Mercado", 
                    EntryType = CategoryType.Variable, 
                    StatementIdentifiers = "SUPERMERCADO|MERCADO|DIA|PÃO DE AÇÚCAR" 
                },
                new Category 
                { 
                    Id = 20, 
                    Description = "Combustível", 
                    EntryType = CategoryType.Variable, 
                    StatementIdentifiers = "POSTO|SHELL|IPIRANGA|PETROBRAS" 
                },
                new Category 
                { 
                    Id = 30, 
                    Description = "Assinaturas & Serviços", 
                    EntryType = CategoryType.Fixed, 
                    StatementIdentifiers = "NETFLIX|SPOTIFY|PRIME VIDEO" 
                }
            };
        }

        [Fact]
        public void IdentifyCategory_ShouldReturnCorrectCategoryId_WhenDescriptionMatchesKeyword()
        {
            var service = new CategoryIdentificationService(context: null!);
            var categories = GetMockCategories();
         
            string transactionDescription = "PAGTO NETFLIX ASSINATURA";

            int? resultId = service.IdentifyCategory(transactionDescription, categories);

            Assert.Equal(30, resultId);
        }

        [Fact]
        public void IdentifyCategory_ShouldBeCaseInsensitive_WhenMatchingKeywords()
        {
            var service = new CategoryIdentificationService(context: null!);
            var categories = GetMockCategories();

            string transactionDescription = "compra no posto shell";

            int? resultId = service.IdentifyCategory(transactionDescription, categories);

            Assert.Equal(20, resultId);
        }

        [Fact]
        public void IdentifyCategory_ShouldReturnNull_WhenNoKeywordsMatchDescription()
        {
            var service = new CategoryIdentificationService(context: null!);
            var categories = GetMockCategories();

            string transactionDescription = "COMPRA LOJA DE ROUPAS ZARA";

            int? resultId = service.IdentifyCategory(transactionDescription, categories);

            Assert.Null(resultId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void IdentifyCategory_ShouldReturnNull_WhenDescriptionIsInvalidOrEmpty(string? invalidDescription)
        {
            var service = new CategoryIdentificationService(context: null!);
            var categories = GetMockCategories();

            int? resultId = service.IdentifyCategory(invalidDescription, categories);

            Assert.Null(resultId);
        }
    }
}