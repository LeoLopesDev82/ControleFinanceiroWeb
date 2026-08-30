using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ControleFinanceiroWeb.Models;
using ControleFinanceiroWeb.Services.Summary;

namespace ControleFinanceiroWeb.Controllers
{
    // Controller responsible for displaying the general financial dashboard and summaries.
    public class SummaryController : Controller
    {
        private readonly ISummaryService _summaryService;

        public SummaryController(ISummaryService summaryService)
        {
            _summaryService = summaryService;
        }

        // Displays the consolidated summary page (revenues, expenses, balance, charts and fixed expenses checklist).
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            var period = DateRange.FromOrCurrentMonth(startDate, endDate);

            var model = await _summaryService.GetSummaryAsync(period.Start, period.End);

            model.Period = period;

            return View(model);
        }
    }
}