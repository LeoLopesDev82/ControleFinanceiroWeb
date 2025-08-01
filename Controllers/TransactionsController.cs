using ControleFinanceiroWeb.Services.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiroWeb.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly MainViewModelBuilder _modelBuilder;

        public TransactionsController(MainViewModelBuilder modelBuilder)
        {
            _modelBuilder = modelBuilder;
        }

        public IActionResult Index(int id, DateTime? startDate, DateTime? endDate)
        {
            var (model, resolvedId, start, end) = _modelBuilder.Build(id, startDate, endDate);

            ViewBag.SelectedId = resolvedId;
            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
			ViewBag.ButtonsInfo = model.ButtonsInfo;
            ViewBag.ToolTitle = model.Title;

			return View(model);
        }
    }
}
