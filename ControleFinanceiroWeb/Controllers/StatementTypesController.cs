using System.Threading.Tasks;
using ControleFinanceiroWeb.Models.ViewModels;
using ControleFinanceiroWeb.Services.StatementType;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiroWeb.Controllers
{
    // Controller responsible for managing statement types (accounts).
    public class StatementTypesController : Controller
    {
        private readonly IStatementTypeService _statementTypeService;

        public StatementTypesController(IStatementTypeService statementTypeService)
        {
            _statementTypeService = statementTypeService;
        }

        // Saves or updates a statement type.
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] StatementTypeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Dados inválidos." });
            }

            var result = await _statementTypeService.SaveStatementTypeAsync(model);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        // Deletes a statement type by id.
        [HttpDelete]
        [Route("StatementTypes/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { success = false, message = "ID inválido." });
            }

            var result = await _statementTypeService.DeleteStatementTypeAsync(id);

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}