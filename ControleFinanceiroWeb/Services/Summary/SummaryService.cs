using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Models.Entities;
using ControleFinanceiroWeb.Models.ViewModels;

namespace ControleFinanceiroWeb.Services.Summary
{
    // Service responsible for computing financial dashboard summaries and data charts.
    public class SummaryService : ISummaryService
    {
        private readonly AppDbContext _context;

        private static readonly string[] ColorPalette = new[]
        {
            "#0f5132", // Forest green
            "#198754", // Emerald green
            "#2e7d32", // Dark green
            "#4caf50", // Green
            "#66bb6a", // Light green
            "#81c784", // Lighter green
            "#a5d6a7", // Pale green
            "#c8e6c9"  // Soft green
        };

        // Initializes a new instance of SummaryService.
        public SummaryService(AppDbContext context)
        {
            _context = context;
        }

        #region Public Methods

        // Generates the comprehensive financial summary view model for dashboards.
        public async Task<SummaryViewModel> GetSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var totalRevenue = await _context.Statement
                .Where(s => s.DueDate >= startDate && s.DueDate <= endDate && s.Amount >= 0)
                .SumAsync(s => (decimal?)s.Amount) ?? 0;

            var totalExpensesRaw = await _context.Statement
                .Where(s => s.DueDate >= startDate && s.DueDate <= endDate && s.Amount < 0)
                .SumAsync(s => (decimal?)s.Amount) ?? 0;

            var totalExpenses = Math.Abs(totalExpensesRaw);
            var totalBalance = totalRevenue - totalExpenses;

            var transactions = await _context.Statement
                .Where(s => s.DueDate >= startDate && s.DueDate <= endDate)
                .ToListAsync();

            var categoriesMap = await _context.Categories.ToDictionaryAsync(c => c.Id);

            CalculateAccumulatedChart(
                transactions,
                startDate,
                endDate,
                out var chartLabels,
                out var chartRevenues,
                out var chartExpenses);

            var expenseDistribution = CalculateExpenseDistribution(transactions, categoriesMap);
            var inflowSummary = CalculateInflowSummary(transactions, categoriesMap);
            var fixedExpenses = await CalculateFixedExpensesChecklistAsync(transactions);

            return new SummaryViewModel
            {
                TotalRevenue = totalRevenue,
                TotalExpenses = totalExpenses,
                TotalBalance = totalBalance,
                ChartLabels = chartLabels,
                ChartRevenues = chartRevenues,
                ChartExpenses = chartExpenses,
                ExpenseDistribution = expenseDistribution,
                InflowSummary = inflowSummary,
                OutflowSummary = expenseDistribution,
                FixedExpenses = fixedExpenses
            };
        }

        #endregion

        #region Private Methods

        // Calculates the accumulated daily totals for credits and debits to render the chart.
        private void CalculateAccumulatedChart(
            List<Statement> transactions,
            DateTime startDate,
            DateTime endDate,
            out List<string> chartLabels,
            out List<decimal> chartRevenues,
            out List<decimal> chartExpenses)
        {
            var grouped = transactions
                .Where(t => t.DueDate.HasValue)
                .GroupBy(t => t.DueDate!.Value.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            chartLabels = new List<string>();
            chartRevenues = new List<decimal>();
            chartExpenses = new List<decimal>();

            decimal accumulatedRevenue = 0;
            decimal accumulatedExpenses = 0;

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (grouped.TryGetValue(date, out var dayTxList))
                {
                    var dayRev = dayTxList.Where(t => t.Amount >= 0).Sum(t => t.Amount ?? 0m);
                    var dayExp = dayTxList.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount ?? 0m));

                    accumulatedRevenue += dayRev;
                    accumulatedExpenses += dayExp;
                }

                chartLabels.Add(date.ToString("dd/MM"));
                chartRevenues.Add(accumulatedRevenue);
                chartExpenses.Add(accumulatedExpenses);
            }
        }

        // Computes distribution details of debit transactions grouped by category.
        private List<CategorySummaryViewModel> CalculateExpenseDistribution(List<Statement> transactions, Dictionary<int, Category> categoriesMap)
        {
            var expenses = transactions.Where(s => s.Amount < 0).ToList();
            var totalExpensesSum = Math.Abs(expenses.Sum(e => e.Amount ?? 0m));

            var groupedExpenses = expenses
                .GroupBy(e => e.EntryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    CategoryName = g.Key.HasValue && categoriesMap.TryGetValue(g.Key.Value, out var cat) ? cat.Description : "Outros",
                    TotalAmount = Math.Abs(g.Sum(e => e.Amount ?? 0m))
                })
                .ToList();

            var categorizedList = groupedExpenses
                .Where(g => g.CategoryId.HasValue)
                .OrderByDescending(g => g.TotalAmount)
                .ToList();

            var outrosItem = groupedExpenses
                .FirstOrDefault(g => !g.CategoryId.HasValue);

            var expenseDistribution = new List<CategorySummaryViewModel>();
            int colorIdx = 0;

            foreach (var item in categorizedList)
            {
                var percentage = totalExpensesSum > 0 ? (double)(item.TotalAmount / totalExpensesSum) * 100 : 0;

                expenseDistribution.Add(new CategorySummaryViewModel
                {
                    CategoryId = item.CategoryId,
                    CategoryName = item.CategoryName,
                    TotalAmount = item.TotalAmount,
                    Percentage = Math.Round(percentage, 1),
                    ColorHex = ColorPalette[colorIdx % ColorPalette.Length]
                });

                colorIdx++;
            }

            if (outrosItem != null && outrosItem.TotalAmount > 0)
            {
                var percentage = totalExpensesSum > 0 ? (double)(outrosItem.TotalAmount / totalExpensesSum) * 100 : 0;

                expenseDistribution.Add(new CategorySummaryViewModel
                {
                    CategoryId = null,
                    CategoryName = "Outros",
                    TotalAmount = outrosItem.TotalAmount,
                    Percentage = Math.Round(percentage, 1),
                    ColorHex = "#6c757d"
                });
            }

            return expenseDistribution;
        }

        // Computes inflow details of credit transactions grouped by category.
        private List<CategorySummaryViewModel> CalculateInflowSummary(List<Statement> transactions, Dictionary<int, Category> categoriesMap)
        {
            var revenuesList = transactions.Where(s => s.Amount >= 0).ToList();
            var totalRevenuesSum = revenuesList.Sum(e => e.Amount ?? 0m);

            var groupedRevenues = revenuesList
                .GroupBy(e => e.EntryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    CategoryName = g.Key.HasValue && categoriesMap.TryGetValue(g.Key.Value, out var cat) ? cat.Description : "Outros",
                    TotalAmount = g.Sum(e => e.Amount ?? 0m)
                })
                .ToList();

            var categorizedRevenues = groupedRevenues
                .Where(g => g.CategoryId.HasValue)
                .OrderByDescending(g => g.TotalAmount)
                .ToList();

            var outrosRevenueItem = groupedRevenues
                .FirstOrDefault(g => !g.CategoryId.HasValue);

            var inflowSummary = new List<CategorySummaryViewModel>();
            int inflowColorIdx = 0;

            foreach (var item in categorizedRevenues)
            {
                var percentage = totalRevenuesSum > 0 ? (double)(item.TotalAmount / totalRevenuesSum) * 100 : 0;

                inflowSummary.Add(new CategorySummaryViewModel
                {
                    CategoryId = item.CategoryId,
                    CategoryName = item.CategoryName,
                    TotalAmount = item.TotalAmount,
                    Percentage = Math.Round(percentage, 2),
                    ColorHex = ColorPalette[inflowColorIdx % ColorPalette.Length]
                });

                inflowColorIdx++;
            }

            if (outrosRevenueItem != null && outrosRevenueItem.TotalAmount > 0)
            {
                var percentage = totalRevenuesSum > 0 ? (double)(outrosRevenueItem.TotalAmount / totalRevenuesSum) * 100 : 0;

                inflowSummary.Add(new CategorySummaryViewModel
                {
                    CategoryId = null,
                    CategoryName = "Outros",
                    TotalAmount = outrosRevenueItem.TotalAmount,
                    Percentage = Math.Round(percentage, 2),
                    ColorHex = "#6c757d"
                });
            }

            return inflowSummary;
        }

        // Computes the checklist tracking for all fixed expense categories in the period.
        private async Task<List<FixedExpenseViewModel>> CalculateFixedExpensesChecklistAsync(List<Statement> transactions)
        {
            var fixedCategories = await _context.Categories
                .Where(c => c.EntryType == CategoryType.Fixed)
                .OrderBy(c => c.Description)
                .ToListAsync();

            var fixedExpenses = new List<FixedExpenseViewModel>();

            foreach (var category in fixedCategories)
            {
                var categoryTransactions = transactions
                    .Where(t => t.EntryId == category.Id && t.Amount < 0)
                    .ToList();

                if (categoryTransactions.Any())
                {
                    var totalAmount = Math.Abs(categoryTransactions.Sum(t => t.Amount ?? 0m));
                    var isPaid = categoryTransactions.All(t => t.TransactionDate.HasValue);

                    var statusText = isPaid
                        ? $"Pago em {categoryTransactions.Max(t => t.TransactionDate!.Value).ToString("dd/MM")}"
                        : "Pendente";

                    fixedExpenses.Add(new FixedExpenseViewModel
                    {
                        ExpenseName = category.Description,
                        CategoryName = category.Description,
                        TotalAmount = totalAmount,
                        StatusText = statusText,
                        IsPaid = isPaid
                    });
                }
                else
                {
                    fixedExpenses.Add(new FixedExpenseViewModel
                    {
                        ExpenseName = category.Description,
                        CategoryName = category.Description,
                        TotalAmount = 0m,
                        StatusText = "Pendente",
                        IsPaid = false
                    });
                }
            }

            return fixedExpenses;
        }

        #endregion
    }
}