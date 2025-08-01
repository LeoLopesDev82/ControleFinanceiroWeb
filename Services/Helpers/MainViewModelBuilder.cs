using ControleFinanceiroDesktop.Models.ViewModels;
using ControleFinanceiroDesktop.Services.Header;

namespace ControleFinanceiroWeb.Services.Helpers
{
    public class MainViewModelBuilder
    {
        private readonly MainViewServices _mainViewServices;

        public MainViewModelBuilder(MainViewServices mainViewServices)
        {
            _mainViewServices = mainViewServices;
        }

        public (MainModel model, int id, string start, string end) Build(int? id, DateTime? startDate, DateTime? endDate)
        {
            DateTime start = startDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime end = endDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));

            int resolvedId = id ?? -1;

            var model = _mainViewServices.GetMainFormModel(start, end, resolvedId);

            return (model, resolvedId, start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));
        }
    }
}
