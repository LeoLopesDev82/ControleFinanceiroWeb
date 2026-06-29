using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControleFinanceiroWeb.Models;
using ControleFinanceiroWeb.Models.ViewModels;

namespace ControleFinanceiroWeb.Services.Transactions
{
    public interface ITransactionService
    {
        Task<TransactionsViewModel> GetTransactionsAsync(int statementTypeId, DateTime startDate, DateTime endDate);
        Task<List<CategoryOptionViewModel>> GetCategoriesAsync();
        Task<TransactionFormViewModel> GetTransactionFormAsync(int id, int statementTypeId);
        Task<ServiceResult> SaveTransactionAsync(TransactionFormViewModel model);
        Task<ServiceResult> DeleteTransactionAsync(int id);
        Task<List<TransactionImportPreviewItemViewModel>> PreviewImportAsync(string rawText);
        Task<ServiceResult> SaveImportAsync(List<TransactionImportSaveModel> items, int statementTypeId);
    }
}