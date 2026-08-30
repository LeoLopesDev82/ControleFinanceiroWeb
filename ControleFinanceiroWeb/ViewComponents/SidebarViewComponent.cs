using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ControleFinanceiroWeb.Models.ViewModels;
using ControleFinanceiroWeb.Services.StatementTypes;

namespace ControleFinanceiroWeb.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly IStatementTypeService _statementTypeService;

        public SidebarViewComponent(IStatementTypeService statementTypeService)
        {
            _statementTypeService = statementTypeService;
        }

        #region Public Methods

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var statementTypes = await _statementTypeService.GetStatementTypesAsync();

            string currentController = RouteData.Values["Controller"]?.ToString() ?? string.Empty;
            string currentStatementType = Request.Query["statementType"].ToString();

            bool isTransactions = currentController == "Transactions";

            var model = new SidebarViewModel
            {
                IsSummaryActive = currentController == "Summary",
                IsExtratoActive = isTransactions
                    && (string.IsNullOrEmpty(currentStatementType) || currentStatementType == "Extrato"),
                Statements = statementTypes
                    .Select(s => new SidebarStatementViewModel
                    {
                        Id = s.Id,
                        Name = s.Name,
                        IsActive = isTransactions && currentStatementType == s.Name
                    })
                    .ToList()
            };

            return View(model);
        }

        #endregion
    }
}
