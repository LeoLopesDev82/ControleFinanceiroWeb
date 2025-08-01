using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Models.DTOs;

namespace ControleFinanceiroWeb.Services.CategoryServices
{
    public class CategoryGetServices
    {
        private readonly AppDbContext _context;

        public CategoryGetServices(AppDbContext context)
        {
            _context = context;
        }

        public CategoryDto? GetDto(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == id);

            if (category == null) return null;

            string radioName = category.EntryType switch
            {
                'F' => "rdbFixed",
                'V' => "rdbVariable",
                _ => string.Empty
            };

            var dto = new CategoryDto
            {
                Id = category.Id,
                Description = category.Description,
                RadioName = radioName,
                Identifiers = category.StatementIdentifiers
            };

            return dto;
        }
    }
}
