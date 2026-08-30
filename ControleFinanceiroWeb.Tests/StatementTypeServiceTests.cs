using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Models.Entities;
using ControleFinanceiroWeb.Services.StatementTypes;

namespace ControleFinanceiroWeb.Tests
{
    public class StatementTypeServiceTests
    {
        #region Public Methods

        [Fact]
        public async Task DeleteStatementTypeAsync_ShouldRefuse_WhenTransactionsStillReferenceTheAccount()
        {
            using var context = InMemoryDatabase.Create();

            context.StatementTypes.Add(new StatementType { Id = 4, Description = "Cartão de Crédito" });
            context.Statement.Add(new Statement { Amount = -120m, Description = "COMPRA", StatementTypeId = 4 });
            
            await context.SaveChangesAsync();

            var result = await CreateService(context).DeleteStatementTypeAsync(4);

            Assert.False(result.Success);
            Assert.Contains("registros vinculados", result.Message);
            Assert.Single(context.StatementTypes);
        }

        [Fact]
        public async Task DeleteStatementTypeAsync_ShouldRemoveTheAccount_WhenNothingReferencesIt()
        {
            using var context = InMemoryDatabase.Create();

            context.StatementTypes.Add(new StatementType { Id = 4, Description = "Conta antiga" });
            
            await context.SaveChangesAsync();

            var result = await CreateService(context).DeleteStatementTypeAsync(4);

            Assert.True(result.Success);
            Assert.Empty(context.StatementTypes);
        }

        [Fact]
        public async Task DeleteStatementTypeAsync_ShouldReportNotFound_ForAnUnknownAccount()
        {
            using var context = InMemoryDatabase.Create();

            var result = await CreateService(context).DeleteStatementTypeAsync(99);

            Assert.False(result.Success);
            Assert.Contains("não foi encontrado", result.Message);
        }

        [Fact]
        public async Task DeleteStatementTypeAsync_ShouldNotBeBlocked_ByTransactionsOfAnotherAccount()
        {
            using var context = InMemoryDatabase.Create();

            context.StatementTypes.Add(new StatementType { Id = 4, Description = "Conta A" });
            context.StatementTypes.Add(new StatementType { Id = 5, Description = "Conta B" });
            context.Statement.Add(new Statement { Amount = -120m, Description = "COMPRA", StatementTypeId = 5 });
            
            await context.SaveChangesAsync();

            var result = await CreateService(context).DeleteStatementTypeAsync(4);

            Assert.True(result.Success);
            Assert.Single(context.StatementTypes);
        }

        #endregion

        #region Private Methods

        private static StatementTypeService CreateService(AppDbContext context)
        {
            return new StatementTypeService(context, NullLogger<StatementTypeService>.Instance);
        }

        #endregion
    }
}