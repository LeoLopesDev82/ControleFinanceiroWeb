using System;
using System.Linq;
using System.Threading.Tasks;
using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Models.Entities;
using ControleFinanceiroWeb.Services.Summary;

namespace ControleFinanceiroWeb.Tests
{
    public class SummaryServiceTests
    {
        private static readonly DateTime Start = new(2026, 3, 1);
        private static readonly DateTime End = new(2026, 3, 31);

        #region Public Methods

        [Fact]
        public async Task GetSummaryAsync_ShouldSeparateRevenueFromExpenses_AndReportExpensesAsPositive()
        {
            using var context = InMemoryDatabase.Create();

            AddStatement(context, new DateTime(2026, 3, 5), 3000m);
            AddStatement(context, new DateTime(2026, 3, 6), -1200m);
            AddStatement(context, new DateTime(2026, 3, 7), -300m);
            
            await context.SaveChangesAsync();

            var result = await CreateService(context).GetSummaryAsync(Start, End);

            Assert.Equal(3000m, result.TotalRevenue);
            Assert.Equal(1500m, result.TotalExpenses);
            Assert.Equal(1500m, result.TotalBalance);
        }

        [Fact]
        public async Task GetSummaryAsync_ShouldReportNegativeBalance_WhenExpensesExceedRevenue()
        {
            using var context = InMemoryDatabase.Create();

            AddStatement(context, new DateTime(2026, 3, 5), 1000m);
            AddStatement(context, new DateTime(2026, 3, 6), -1750m);
            
            await context.SaveChangesAsync();

            var result = await CreateService(context).GetSummaryAsync(Start, End);

            Assert.Equal(-750m, result.TotalBalance);
        }

        [Fact]
        public async Task GetSummaryAsync_ShouldIgnoreTransactions_OutsideTheSelectedPeriod()
        {
            using var context = InMemoryDatabase.Create();

            AddStatement(context, new DateTime(2026, 2, 27), 5000m);
            AddStatement(context, new DateTime(2026, 3, 10), 1000m);
            AddStatement(context, new DateTime(2026, 4, 2), 7000m);
            
            await context.SaveChangesAsync();

            var result = await CreateService(context).GetSummaryAsync(Start, End);

            Assert.Equal(1000m, result.TotalRevenue);
        }

        [Fact]
        public async Task GetSummaryAsync_ShouldGroupUncategorisedExpenses_UnderOutros()
        {
            using var context = InMemoryDatabase.Create();

            context.Categories.Add(new Category { Id = 1, Description = "Moradia", EntryType = CategoryType.Fixed, StatementIdentifiers = "ALUGUEL" });

            AddStatement(context, new DateTime(2026, 3, 5), -800m, categoryId: 1);
            AddStatement(context, new DateTime(2026, 3, 6), -200m, categoryId: null);
            
            await context.SaveChangesAsync();

            var result = await CreateService(context).GetSummaryAsync(Start, End);
            var outros = result.ExpenseDistribution.Single(e => e.CategoryName == "Outros");

            Assert.Equal(200m, outros.TotalAmount);
            Assert.Null(outros.CategoryId);
        }

        [Fact]
        public async Task GetSummaryAsync_ShouldSplitExpensesByCategoryShare()
        {
            using var context = InMemoryDatabase.Create();

            context.Categories.Add(new Category { Id = 1, Description = "Moradia", EntryType = CategoryType.Fixed, StatementIdentifiers = "ALUGUEL" });
            context.Categories.Add(new Category { Id = 2, Description = "Lazer", EntryType = CategoryType.Variable, StatementIdentifiers = "CINEMA" });

            AddStatement(context, new DateTime(2026, 3, 5), -750m, categoryId: 1);
            AddStatement(context, new DateTime(2026, 3, 6), -250m, categoryId: 2);
            
            await context.SaveChangesAsync();

            var result = await CreateService(context).GetSummaryAsync(Start, End);

            Assert.Equal(75d, result.ExpenseDistribution.Single(e => e.CategoryName == "Moradia").Percentage);
            Assert.Equal(25d, result.ExpenseDistribution.Single(e => e.CategoryName == "Lazer").Percentage);
        }

        [Fact]
        public async Task GetSummaryAsync_ShouldOrderExpenseDistribution_ByAmountDescending()
        {
            using var context = InMemoryDatabase.Create();

            context.Categories.Add(new Category { Id = 1, Description = "Pequena", EntryType = CategoryType.Variable, StatementIdentifiers = "X" });
            context.Categories.Add(new Category { Id = 2, Description = "Grande", EntryType = CategoryType.Variable, StatementIdentifiers = "Y" });

            AddStatement(context, new DateTime(2026, 3, 5), -100m, categoryId: 1);
            AddStatement(context, new DateTime(2026, 3, 6), -900m, categoryId: 2);
            
            await context.SaveChangesAsync();

            var result = await CreateService(context).GetSummaryAsync(Start, End);

            Assert.Equal("Grande", result.ExpenseDistribution.First().CategoryName);
        }

        [Fact]
        public async Task GetSummaryAsync_ShouldListFixedCategoryAsPending_WhenItHasNoTransaction()
        {
            using var context = InMemoryDatabase.Create();

            context.Categories.Add(new Category { Id = 1, Description = "Condomínio", EntryType = CategoryType.Fixed, StatementIdentifiers = "CONDOMINIO" });
            
            await context.SaveChangesAsync();

            var result = await CreateService(context).GetSummaryAsync(Start, End);
            var item = Assert.Single(result.FixedExpenses);

            Assert.False(item.IsPaid);
            Assert.Equal("Pendente", item.StatusText);
            Assert.Equal(0m, item.TotalAmount);
        }

        [Fact]
        public async Task GetSummaryAsync_ShouldMarkFixedExpenseAsPaid_WhenTheTransactionHasBeenSettled()
        {
            using var context = InMemoryDatabase.Create();

            context.Categories.Add(new Category { Id = 1, Description = "Condomínio", EntryType = CategoryType.Fixed, StatementIdentifiers = "CONDOMINIO" });

            AddStatement(context, new DateTime(2026, 3, 10), -450m, categoryId: 1, settled: true);
            
            await context.SaveChangesAsync();

            var result = await CreateService(context).GetSummaryAsync(Start, End);
            var item = Assert.Single(result.FixedExpenses);

            Assert.True(item.IsPaid);
            Assert.Equal(450m, item.TotalAmount);
            Assert.StartsWith("Pago em", item.StatusText);
        }

        [Fact]
        public async Task GetSummaryAsync_ShouldKeepFixedExpensePending_WhenTheTransactionHasNoSettlementDate()
        {
            using var context = InMemoryDatabase.Create();

            context.Categories.Add(new Category { Id = 1, Description = "Seguro", EntryType = CategoryType.Fixed, StatementIdentifiers = "SEGURO" });

            AddStatement(context, new DateTime(2026, 3, 20), -180m, categoryId: 1, settled: false);
            await context.SaveChangesAsync();

            var result = await CreateService(context).GetSummaryAsync(Start, End);
            var item = Assert.Single(result.FixedExpenses);

            Assert.False(item.IsPaid);
            Assert.Equal("Pendente", item.StatusText);
            Assert.Equal(180m, item.TotalAmount);
        }

        [Fact]
        public async Task GetSummaryAsync_ShouldExcludeVariableCategories_FromTheFixedExpenseChecklist()
        {
            using var context = InMemoryDatabase.Create();

            context.Categories.Add(new Category { Id = 1, Description = "Aluguel", EntryType = CategoryType.Fixed, StatementIdentifiers = "ALUGUEL" });
            context.Categories.Add(new Category { Id = 2, Description = "Lazer", EntryType = CategoryType.Variable, StatementIdentifiers = "CINEMA" });
            
            await context.SaveChangesAsync();

            var result = await CreateService(context).GetSummaryAsync(Start, End);

            Assert.Single(result.FixedExpenses);
            Assert.Equal("Aluguel", result.FixedExpenses.Single().CategoryName);
        }

        [Fact]
        public async Task GetSummaryAsync_ShouldAccumulateChartTotals_AcrossThePeriod()
        {
            using var context = InMemoryDatabase.Create();

            AddStatement(context, new DateTime(2026, 3, 1), 1000m);
            AddStatement(context, new DateTime(2026, 3, 2), -400m);
            
            await context.SaveChangesAsync();

            var result = await CreateService(context).GetSummaryAsync(Start, End);

            Assert.Equal(31, result.ChartLabels.Count);
            Assert.Equal(1000m, result.ChartRevenues.First());
            Assert.Equal(1000m, result.ChartRevenues.Last());
            Assert.Equal(400m, result.ChartExpenses.Last());
        }

        [Fact]
        public async Task GetSummaryAsync_ShouldReturnZeroedTotals_WhenThereIsNoData()
        {
            using var context = InMemoryDatabase.Create();

            var result = await CreateService(context).GetSummaryAsync(Start, End);

            Assert.Equal(0m, result.TotalRevenue);
            Assert.Equal(0m, result.TotalExpenses);
            Assert.Equal(0m, result.TotalBalance);
            Assert.Empty(result.ExpenseDistribution);
        }

        #endregion

        #region Private Methods

        private static SummaryService CreateService(AppDbContext context)
        {
            return new SummaryService(context);
        }

        private static void AddStatement(
            AppDbContext context,
            DateTime dueDate,
            decimal amount,
            int? categoryId = null,
            bool settled = true)
        {
            context.Statement.Add(new Statement
            {
                DueDate = dueDate,
                TransactionDate = settled ? dueDate : null,
                Amount = amount,
                Description = "Lançamento de teste",
                EntryId = categoryId,
                StatementTypeId = 0
            });
        }

        #endregion
    }
}