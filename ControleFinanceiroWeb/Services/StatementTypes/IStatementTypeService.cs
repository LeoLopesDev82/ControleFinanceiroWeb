using System.Collections.Generic;
using System.Threading.Tasks;
using ControleFinanceiroWeb.Models;
using ControleFinanceiroWeb.Models.ViewModels;

namespace ControleFinanceiroWeb.Services.StatementTypes
{
    public interface IStatementTypeService
    {
        Task<List<StatementTypeViewModel>> GetStatementTypesAsync();
        Task<ServiceResult> SaveStatementTypeAsync(StatementTypeViewModel model);
        Task<ServiceResult> DeleteStatementTypeAsync(int id);
    }
}
