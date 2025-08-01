using ControleFinanceiroWeb.Services.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiroWeb.Controllers
{
    public class SummaryController : Controller
    {
        private readonly MainViewModelBuilder _modelBuilder;

        public SummaryController(MainViewModelBuilder modelBuilder)
        {
            _modelBuilder = modelBuilder;
        }

        public IActionResult Index(DateTime? startDate, DateTime? endDate)
        {
            var (model, resolvedId, start, end) = _modelBuilder.Build(null, startDate, endDate);

            ViewBag.SelectedId = resolvedId;
            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
			ViewBag.ButtonsInfo = model.ButtonsInfo;
            ViewBag.ToolTitle = model.Title;

            return View(model);
        }
    }
}
