using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Models.DTOs;
using ControleFinanceiroWeb.Models.Entities;
using ControleFinanceiroWeb.Responses;
using System.Net;
using System.Text;

namespace ControleFinanceiroWeb.Services.StatementServices
{
    public class SatatementSaveServices
    {
        private readonly AppDbContext _context;

        public SatatementSaveServices(AppDbContext context)
        {
            _context = context;
        }

        public SaveResult Save(StatementDto dto)
        {
            try
            {
                var validateData = ValidateData(dto);

                if (!validateData.Success) return validateData;

                return dto.Id.HasValue ? Update(dto) : Insert(dto);
            }
            catch (Exception)
            {
                return new SaveResult
                {
                    Success = false,
                    Message = "Ocorreu um erro inesperado e não foi possível criar ou atualizar a linha do extrato.",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        #region "subroutines"

        public SaveResult ValidateData(StatementDto dto)
        {
            var errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(dto.Description))
                errors.AppendLine("• A descrição é obrigatória.");
            else if (dto.Description.Length > 255)
                errors.AppendLine("• A descrição não pode ter mais que 255 caracteres.");

            if (!dto.TransactionDate.HasValue) errors.AppendLine("• A data da transação é obrigatória.");
            if (!dto.DueDate.HasValue) errors.AppendLine("• A data de vencimento é obrigatória.");
            if (!dto.Amount.HasValue) errors.AppendLine("• O valor é obrigatório.");

            bool success = errors.Length == 0;
            string message = success ? "" : $"Não é possível salvar:\n\n{errors.ToString().Trim()}";

            return new SaveResult
            {
                Success = success,
                Message = message,
                StatusCode = success ? HttpStatusCode.OK : HttpStatusCode.BadRequest
            };
        }

        public SaveResult Insert(StatementDto dto)
        {
            var entity = new Statement();

            MapDtoToEntity(dto, entity);

            _context.Statement.Add(entity);
            _context.SaveChanges();

            return new SaveResult
            {
                Success = true,
                Id = entity.Id,
                Message = "Registro inserido com sucesso.",
                StatusCode = HttpStatusCode.Created
            };
        }

        public SaveResult Update(StatementDto dto)
        {
            var entity = _context.Statement.FirstOrDefault(s => s.Id == dto.Id);

            if (entity == null)
            {
                return new SaveResult
                {
                    Success = false,
                    Message = "Registro não encontrado para atualização.",
                    StatusCode = HttpStatusCode.NotFound
                };
            }

            MapDtoToEntity(dto, entity);

            _context.Statement.Update(entity);
            _context.SaveChanges();

            return new SaveResult
            {
                Success = true,
                Id = entity.Id,
                Message = "Registro atualizado com sucesso.",
                StatusCode = HttpStatusCode.OK
            };
        }
        
        private void MapDtoToEntity(StatementDto dto, Statement entity)
        {
            entity.TransactionDate = dto.TransactionDate;
            entity.DueDate = dto.DueDate;
            entity.Amount = dto.Amount;
            entity.Description = dto.Description;
            entity.EntryId = dto.EntryId;
            entity.StatementTypeId = dto.StatementTypeId;
        }

        #endregion
    }
}