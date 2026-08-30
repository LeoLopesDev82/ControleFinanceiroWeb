using System.Threading.Tasks;
using ControleFinanceiroWeb.Models;

namespace ControleFinanceiroWeb.Services.Security
{
    public interface ISecurityService
    {
        Task<bool> IsPinConfiguredAsync();

        Task<ServiceResult> DefinePinAsync(string pin);

        Task<ServiceResult> ValidatePinAsync(string pin);
    }
}
