using System;
using System.Threading.Tasks;
using ControleFinanceiroWeb.Models.ViewModels;

namespace ControleFinanceiroWeb.Services.Summary
{
    public interface ISummaryService
    {
        Task<SummaryViewModel> GetSummaryAsync(DateTime startDate, DateTime endDate);
    }
}