using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
            DateTime start = startDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime end = endDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
            
            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");

            var model = await _summaryService.GetSummaryAsync(start, end);

            return View(model);
        }
    }
}