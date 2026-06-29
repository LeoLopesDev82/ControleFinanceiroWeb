using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControleFinanceiroWeb.Models;
using ControleFinanceiroWeb.Models.Entities;

namespace ControleFinanceiroWeb.Services.Categories
{
    public interface ICategoryIdentificationService
    {
        int? IdentifyCategory(string? description, IEnumerable<Category> categories);
        Task<ServiceResult> IdentifyTransactionsAsync(int statementTypeId, DateTime startDate, DateTime endDate);
    }
}