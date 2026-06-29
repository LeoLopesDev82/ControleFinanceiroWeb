using System.Collections.Generic;

namespace ControleFinanceiroWeb.Models.ViewModels
{
    public class SummaryViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalBalance { get; set; }

        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> ChartRevenues { get; set; } = new();
        public List<decimal> ChartExpenses { get; set; } = new();

        public List<CategorySummaryViewModel> ExpenseDistribution { get; set; } = new();

        public List<CategorySummaryViewModel> InflowSummary { get; set; } = new();
        public List<CategorySummaryViewModel> OutflowSummary { get; set; } = new();

        public List<FixedExpenseViewModel> FixedExpenses { get; set; } = new();
    }

    public class CategorySummaryViewModel
    {
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = "Outros";
        public decimal TotalAmount { get; set; }
        public double Percentage { get; set; }
        public string ColorHex { get; set; } = "#6c757d";
    }

    public class FixedExpenseViewModel
    {
        public string ExpenseName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string StatusText { get; set; } = "Pendente";
        public bool IsPaid { get; set; }
    }
}