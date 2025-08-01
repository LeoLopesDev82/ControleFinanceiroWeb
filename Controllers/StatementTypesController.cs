using ControleFinanceiroWeb.Models.DTOs;
using ControleFinanceiroWeb.Responses;
using ControleFinanceiroWeb.Services.StatementTypeServices;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiroWeb.Controllers
{
    public class StatementTypesController : Controller
    {
        private readonly StatementTypeSaveServices _saveService;
        private readonly StatementTypeDeleteServices _deleteService;

        public StatementTypesController(StatementTypeSaveServices saveService, StatementTypeDeleteServices deleteService)
        {
            _saveService = saveService;
            _deleteService = deleteService;
        }

        [HttpPost]
        public IActionResult Save([FromBody] StatementTypeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Dados inválidos." });

            SaveResult result = _saveService.SaveStatementType(dto.Description, dto.Id);

            return StatusCode((int)result.StatusCode, result);
        }

        [HttpDelete]
        [Route("StatementTypes/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            if (id <= 0) return BadRequest(new { success = false, message = "ID inválido." });

            SaveResult result = _deleteService.DeleteStatementType(id);

            return StatusCode((int)result.StatusCode, result);
        }
    }
}
