using ControleFinanceiroWeb.Models.DTOs;
using ControleFinanceiroWeb.Responses;
using ControleFinanceiroWeb.Services.StatementServices;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ControleFinanceiroWeb.Controllers
{
    [Route("Statements")]
    public class StatementsController : ControllerBase
    {
        private readonly SatatementSaveServices _saveService;
        private readonly StatementDeleteServices _deleteService;
        private readonly StatementExcelImportService _importService;
        private readonly CategoryReidentificationServices _reidentificationService;

        public StatementsController(
            SatatementSaveServices saveService,
            StatementDeleteServices deleteService,
            StatementExcelImportService importService,
            CategoryReidentificationServices reidentificationService)
        {
            _saveService = saveService;
            _deleteService = deleteService;
            _importService = importService;
            _reidentificationService = reidentificationService;
        }

        [HttpPost("save")]
        public IActionResult Save([FromBody] StatementDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Dados inválidos." });

            SaveResult result = _saveService.Save(dto);

            return StatusCode((int)result.StatusCode, result);
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            if (id <= 0) return BadRequest(new { success = false, message = "ID inválido." });

            SaveResult result = _deleteService.Delete(id);

            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("import")]
        public IActionResult Import([FromForm] IFormFile file, [FromForm] int statementTypeId)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new SaveResult
                {
                    Success = false,
                    Message = "Nenhum arquivo foi enviado.",
                    StatusCode = HttpStatusCode.BadRequest
                });

            string tempFilePath = Path.GetTempFileName();

            try
            {
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var result = _importService.DoImport(tempFilePath, statementTypeId);

                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new SaveResult
                {
                    Success = false,
                    Message = "Erro ao processar o arquivo: " + ex.Message,
                    StatusCode = HttpStatusCode.InternalServerError
                });
            }
            finally
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
        }

        [HttpPost("reidentify")]
        public IActionResult Reidentify([FromBody] ReidentifyDto request)
        {
            if (request.StartDate == null || request.EndDate == null)
            {
                return BadRequest(new SaveResult
                {
                    Success = false,
                    Message = "Período inválido informado.",
                    StatusCode = HttpStatusCode.BadRequest
                });
            }

            bool success = _reidentificationService.Reidentify(request.StartDate.Value, request.EndDate.Value);

            if (success)
            {
                return Ok(new SaveResult
                {
                    Success = true,
                    Message = "Registros atualizados com sucesso.",
                    StatusCode = HttpStatusCode.OK
                });
            }

            return StatusCode(500, new SaveResult
            {
                Success = false,
                Message = "Erro ao reidentificar as categorias.",
                StatusCode = HttpStatusCode.InternalServerError
            });
        }
    }
}
