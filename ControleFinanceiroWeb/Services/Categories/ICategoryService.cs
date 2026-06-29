using System.Collections.Generic;
using System.Threading.Tasks;
using ControleFinanceiroWeb.Models;
using ControleFinanceiroWeb.Models.ViewModels;

namespace ControleFinanceiroWeb.Services.Categories
{
    public interface ICategoryService
    {
        Task<List<CategoryListViewModel>> GetCategoriesForListAsync();
        Task<List<CategoryOptionViewModel>> GetCategoriesForOptionAsync();
        Task<CategoryFormViewModel> GetCategoryAsync(int id);
        Task<ServiceResult> SaveCategoryAsync(CategoryFormViewModel model);
        Task<ServiceResult> DeleteCategoryAsync(int id);
    }
}
