using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Models.Entities;
using ControleFinanceiroWeb.Models.ViewModels;
using ControleFinanceiroWeb.Services.Categories;
using ControleFinanceiroWeb.Services.Transactions;

namespace ControleFinanceiroWeb.Tests
{
    public class TransactionImportTests
    {
        #region Public Methods

        [Fact]
        public async Task PreviewImportAsync_ShouldParseAValidLine()
        {
            using var context = InMemoryDatabase.Create();

            var result = await CreateService(context)
                .PreviewImportAsync("05/03/2026\t10/03/2026\tSUPERMERCADO CENTRAL\t-250,75");

            var item = Assert.Single(result);

            Assert.True(item.IsValid);
            Assert.Equal("2026-03-05", item.ParsedDate);
            Assert.Equal("2026-03-10", item.ParsedDueDate);
            Assert.Equal(-250.75m, item.ParsedAmount);
            Assert.Equal("SUPERMERCADO CENTRAL", item.Description);
        }

        [Fact]
        public async Task PreviewImportAsync_ShouldRejectALine_WithFewerThanFourColumns()
        {
            using var context = InMemoryDatabase.Create();

            var result = await CreateService(context)
                .PreviewImportAsync("05/03/2026\tSUPERMERCADO\t-100,00");

            var item = Assert.Single(result);

            Assert.False(item.IsValid);
            Assert.Contains("Linha incompleta", item.ErrorMessage);
        }

        [Fact]
        public async Task PreviewImportAsync_ShouldFlagAnInvalidDate_WithoutDiscardingTheOtherLines()
        {
            using var context = InMemoryDatabase.Create();

            var raw = "NAO_E_DATA\t10/03/2026\tPADARIA\t-30,00\n"
                    + "06/03/2026\t06/03/2026\tPOSTO\t-180,00";

            var result = await CreateService(context).PreviewImportAsync(raw);

            Assert.Equal(2, result.Count);
            Assert.False(result[0].IsValid);
            Assert.Contains("Data de movimentação inválida", result[0].ErrorMessage);
            Assert.True(result[1].IsValid);
        }

        [Fact]
        public async Task PreviewImportAsync_ShouldFlagAnInvalidAmount()
        {
            using var context = InMemoryDatabase.Create();

            var result = await CreateService(context)
                .PreviewImportAsync("05/03/2026\t05/03/2026\tPADARIA\tABC");

            var item = Assert.Single(result);

            Assert.False(item.IsValid);
            Assert.Contains("Valor inválido", item.ErrorMessage);
        }

        [Fact]
        public async Task PreviewImportAsync_ShouldSkipBlankLines()
        {
            using var context = InMemoryDatabase.Create();

            var raw = "05/03/2026\t05/03/2026\tPADARIA\t-30,00\n"
                    + "\n"
                    + "   \n"
                    + "06/03/2026\t06/03/2026\tPOSTO\t-180,00";

            var result = await CreateService(context).PreviewImportAsync(raw);

            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].RowIndex);
            Assert.Equal(2, result[1].RowIndex);
        }

        [Fact]
        public async Task PreviewImportAsync_ShouldAssignACategory_WhenAKeywordMatchesTheDescription()
        {
            using var context = InMemoryDatabase.Create();

            context.Categories.Add(new Category
            {
                Id = 7,
                Description = "Alimentação",
                EntryType = CategoryType.Variable,
                StatementIdentifiers = "SUPERMERCADO|PADARIA"
            });
            await context.SaveChangesAsync();

            var result = await CreateService(context)
                .PreviewImportAsync("05/03/2026\t05/03/2026\tPADARIA DO BAIRRO\t-42,90");

            var item = Assert.Single(result);

            Assert.Equal(7, item.CategoryId);
            Assert.Equal("Alimentação", item.CategoryName);
        }

        [Fact]
        public async Task PreviewImportAsync_ShouldLeaveTheCategoryUndefined_WhenNoKeywordMatches()
        {
            using var context = InMemoryDatabase.Create();

            context.Categories.Add(new Category
            {
                Id = 7,
                Description = "Alimentação",
                EntryType = CategoryType.Variable,
                StatementIdentifiers = "SUPERMERCADO"
            });

            await context.SaveChangesAsync();

            var result = await CreateService(context)
                .PreviewImportAsync("05/03/2026\t05/03/2026\tOFICINA MECANICA\t-500,00");

            var item = Assert.Single(result);

            Assert.Null(item.CategoryId);
            Assert.Equal("Não definida", item.CategoryName);
        }

        [Fact]
        public async Task PreviewImportAsync_ShouldReturnNothing_WhenTheClipboardIsEmpty()
        {
            using var context = InMemoryDatabase.Create();

            var result = await CreateService(context).PreviewImportAsync("   ");

            Assert.Empty(result);
        }

        [Fact]
        public async Task SaveImportAsync_ShouldPersistTheSelectedItems_AgainstTheGivenAccount()
        {
            using var context = InMemoryDatabase.Create();

            var items = new List<TransactionImportSaveModel>
            {
                new() { TransactionDate = "2026-03-05", DueDate = "2026-03-10", Description = "PADARIA", Amount = "-30,00", CategoryId = 0 },
                new() { TransactionDate = "2026-03-06", DueDate = "2026-03-06", Description = "POSTO", Amount = "-180,00", CategoryId = 0 }
            };

            var result = await CreateService(context).SaveImportAsync(items, statementTypeId: 3);

            Assert.True(result.Success);
            Assert.Equal(2, context.Statement.Count());
            Assert.All(context.Statement, s => Assert.Equal(3, s.StatementTypeId));
        }

        [Fact]
        public async Task SaveImportAsync_ShouldRefuseAnEmptyList()
        {
            using var context = InMemoryDatabase.Create();

            var result = await CreateService(context).SaveImportAsync(new List<TransactionImportSaveModel>(), statementTypeId: 1);

            Assert.False(result.Success);
            Assert.Empty(context.Statement);
        }

        #endregion

        #region Private Methods

        private static TransactionService CreateService(AppDbContext context)
        {
            var identification = new CategoryIdentificationService(context, NullLogger<CategoryIdentificationService>.Instance);

            return new TransactionService(context, identification, NullLogger<TransactionService>.Instance);
        }

        #endregion
    }
}