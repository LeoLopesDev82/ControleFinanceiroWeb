using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Models;
using ControleFinanceiroWeb.Models.Entities;
using ControleFinanceiroWeb.Models.ViewModels;

namespace ControleFinanceiroWeb.Services.Categories
{
    // Service responsible for handling CRUD business operations for finance categories.
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        // Initializes a new instance of CategoryService.
        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        #region Public Methods

        // Fetches all categories formatted for table lists.
        public async Task<List<CategoryListViewModel>> GetCategoriesForListAsync()
        {
            var rawList = await _context.Categories
                .OrderBy(c => c.Description)
                .ToListAsync();

            return rawList.Select(c => new CategoryListViewModel
            {
                Id = c.Id,
                Description = c.Description,
                EntryType = c.EntryType == CategoryType.Fixed ? "Fixo" : "Variável",
                StatementIdentifiers = c.StatementIdentifiers ?? string.Empty
            }).ToList();
        }

        // Fetches all categories formatted for options dropdowns.
        public async Task<List<CategoryOptionViewModel>> GetCategoriesForOptionAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Description)
                .Select(c => new CategoryOptionViewModel
                {
                    Id = c.Id,
                    Description = c.Description,
                    EntryType = (char)c.EntryType
                })
                .ToListAsync();
        }

        // Fetches a single category for edit form by id, returning default creation model if not found or id <= 0.
        public async Task<CategoryFormViewModel> GetCategoryAsync(int id)
        {
            if (id <= 0) return CreateDefaultCategoryFormViewModel();

            var c = await _context.Categories.FirstOrDefaultAsync(cat => cat.Id == id);

            if (c == null) return CreateDefaultCategoryFormViewModel();

            return new CategoryFormViewModel
            {
                Id = c.Id.ToString(),
                Description = c.Description,
                EntryType = ((char)c.EntryType).ToString(),
                StatementIdentifiers = c.StatementIdentifiers
            };
        }

        // Validates and saves (inserts or updates) a category.
        public async Task<ServiceResult> SaveCategoryAsync(CategoryFormViewModel model)
        {
            var validate = Helpers.ModelValidator.Validate(model);

            if (!validate.Success) return validate;

            if (!IsEntryTypeValid(model.EntryType))
            {
                return new ServiceResult { Success = false, Message = "• O tipo de lançamento é inválido." };
            }

            CategoryType entryType = model.EntryType == "F" ? CategoryType.Fixed : CategoryType.Variable;

            int id = Helpers.ConversionHelper.ToNullableInt(model.Id) ?? 0;

            try
            {
                if (id == 0)
                    return await InsertCategoryAsync(model, entryType);
                else
                    return await UpdateCategoryAsync(model, id, entryType);
            }
            catch (Exception)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Ocorreu um erro inesperado ao salvar os dados no banco de dados."
                };
            }
        }

        // Deletes a category if it is not referenced by any existing transactions.
        public async Task<ServiceResult> DeleteCategoryAsync(int id)
        {
            try
            {
                if (await IsCategoryReferencedAsync(id))
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "• Esta categoria não pode ser excluída pois está associada a uma ou mais movimentações."
                    };
                }

                var existing = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

                if (existing == null)
                {
                    return new ServiceResult { Success = false, Message = "• Registro não encontrado para exclusão." };
                }

                _context.Categories.Remove(existing);
                await _context.SaveChangesAsync();

                return new ServiceResult { Success = true, Message = "Categoria excluída com sucesso." };
            }
            catch (Exception)
            {
                return new ServiceResult { Success = false, Message = "Ocorreu um erro ao excluir a categoria." };
            }
        }

        #endregion

        #region Private Methods

        // Creates a default CategoryFormViewModel for category creation forms.
        private CategoryFormViewModel CreateDefaultCategoryFormViewModel()
        {
            return new CategoryFormViewModel
            {
                Id = "0",
                EntryType = "F"
            };
        }

        // Validates whether the entry type string is a valid category type (F or V).
        private bool IsEntryTypeValid(string? entryType)
        {
            return !string.IsNullOrWhiteSpace(entryType) && (entryType == "F" || entryType == "V");
        }

        // Maps properties from the view model to a category database entity.
        private void MapViewModelToEntity(CategoryFormViewModel model, Models.Entities.Category entity, CategoryType entryType)
        {
            entity.Description = Helpers.ConversionHelper.ToNullableString(model.Description) ?? string.Empty;
            entity.EntryType = entryType;
            entity.StatementIdentifiers = Helpers.ConversionHelper.ToNullableString(model.StatementIdentifiers) ?? string.Empty;
        }

        // Inserts a new category into the database.
        private async Task<ServiceResult> InsertCategoryAsync(CategoryFormViewModel model, CategoryType entryType)
        {
            var newCat = new Models.Entities.Category();

            MapViewModelToEntity(model, newCat, entryType);

            _context.Categories.Add(newCat);
            await _context.SaveChangesAsync();

            return new ServiceResult
            {
                Success = true,
                Message = "Categoria incluída com sucesso!",
                Id = newCat.Id
            };
        }

        // Updates an existing category in the database.
        private async Task<ServiceResult> UpdateCategoryAsync(CategoryFormViewModel model, int id, CategoryType entryType)
        {
            var existing = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null)
            {
                return new ServiceResult { Success = false, Message = "• Registro não encontrado para edição." };
            }

            MapViewModelToEntity(model, existing, entryType);

            await _context.SaveChangesAsync();

            return new ServiceResult
            {
                Success = true,
                Message = "Categoria atualizada com sucesso!",
                Id = existing.Id
            };
        }

        // Checks if a category is currently referenced by any transaction statements.
        private async Task<bool> IsCategoryReferencedAsync(int id)
        {
            return await _context.Statement.AnyAsync(s => s.EntryId == id);
        }

        #endregion
    }
}