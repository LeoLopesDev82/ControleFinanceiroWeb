using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Models.DTOs;

namespace ControleFinanceiroWeb.Services.CategoryServices
{
    public class CategoryListServices
    {
        private readonly AppDbContext _context;

        public CategoryListServices(AppDbContext context)
        {
            _context = context;
        }

        public List<ListItemDto> GetList()
        {
            return _context.Categories
                .OrderBy(c => c.Description)
                .Select(c => new ListItemDto
                {
                    Id = c.Id,
                    Description = c.Description
                })
                .ToList();
        }
    }
}
