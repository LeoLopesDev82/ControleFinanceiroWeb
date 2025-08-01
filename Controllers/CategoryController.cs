using ControleFinanceiroWeb.Services.CategoryServices;
using ControleFinanceiroWeb.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using ControleFinanceiroWeb.Responses;

namespace ControleFinanceiroWeb.Controllers.Api
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryListServices _categoryListServices;
		private readonly CategoryDeleteServices _deleteService;
        private readonly CategoryGetServices _getService;
        private readonly CategorySaveServices _saveService;

        public CategoryController(
            CategoryListServices categoryListServices,
            CategoryDeleteServices deleteService,
            CategoryGetServices getService,
            CategorySaveServices saveService)
        {
            _categoryListServices = categoryListServices;
            _deleteService = deleteService;
            _getService = getService;
            _saveService = saveService;
        }

        [HttpGet]
        public ActionResult<List<ListItemDto>> GetAll()
        {
            var categories = _categoryListServices.GetList();

            return Ok(categories);
        }

		[HttpDelete("Delete/{id}")]
		public IActionResult Delete(int id)
		{
			if (id <= 0)
			{
				return BadRequest(new SaveResult
				{
					Success = false,
					Message = "ID inválido.",
					StatusCode = System.Net.HttpStatusCode.BadRequest
				});
			}

			SaveResult result = _deleteService.Delete(id);

			return StatusCode((int)result.StatusCode, result);
		}

        [HttpGet("{id}")]
        public ActionResult<CategoryDto> GetById(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            var dto = _getService.GetDto(id);

            if (dto == null) return NotFound("Categoria não encontrada.");

            return Ok(dto);
        }

        [HttpPost("save")]
        public IActionResult Save([FromBody] CategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Dados inválidos." });

            SaveResult result = _saveService.Save(dto.Id, dto.Description, dto.RadioName, dto.Identifiers);

            return StatusCode((int)result.StatusCode, result);
        }
    }
}
