using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ControleFinanceiroWeb.Services.Categories;
using ControleFinanceiroWeb.Models.ViewModels;

namespace ControleFinanceiroWeb.Controllers
{
    // Controller responsible for managing finance categories.
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // Returns the category list partial view for grid render.
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var list = await _categoryService.GetCategoriesForListAsync();

            return PartialView("_CategoryList", list);
        }

        // Returns all categories as a JSON option list for dropdowns.
        [HttpGet]
        public async Task<IActionResult> GetCategoriesJson()
        {
            var list = await _categoryService.GetCategoriesForOptionAsync();

            return Ok(list);
        }

        // Returns the insertion/editing category form partial view.
        [HttpGet]
        public async Task<IActionResult> GetForm(int id)
        {
            var model = await _categoryService.GetCategoryAsync(id);

            return PartialView("_CategoryForm", model);
        }

        // Saves or updates a category.
        [HttpPost]
        public async Task<IActionResult> Save(CategoryFormViewModel model)
        {
            var result = await _categoryService.SaveCategoryAsync(model);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        // Deletes a category by id.
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}