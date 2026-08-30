using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Models;
using ControleFinanceiroWeb.Models.Entities;
using ControleFinanceiroWeb.Models.ViewModels;

namespace ControleFinanceiroWeb.Services.StatementType
{
    // Service responsible for handling CRUD business operations for account statement types.
    public class StatementTypeService : IStatementTypeService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StatementTypeService> _logger;

        // Initializes a new instance of StatementTypeService.
        public StatementTypeService(AppDbContext context, ILogger<StatementTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Public Methods

        // Fetches all statement types (accounts) formatted for view models.
        public async Task<List<StatementTypeViewModel>> GetStatementTypesAsync()
        {
            return await _context.StatementTypes
                .OrderBy(s => s.Id)
                .Select(s => new StatementTypeViewModel
                {
                    Id = s.Id,
                    Name = s.Description ?? string.Empty
                })
                .ToListAsync();
        }

        // Validates and saves (inserts or updates) a statement type.
        public async Task<ServiceResult> SaveStatementTypeAsync(StatementTypeViewModel model)
        {
            var validate = Helpers.ModelValidator.Validate(model);

            if (!validate.Success) return validate;

            if (await DoesStatementTypeExistAsync(model.Name, model.Id))
            {
                return new ServiceResult { Success = false, Message = "• Já existe outro registro com a mesma descrição." };
            }

            return model.Id == 0 ? await InsertAsync(model.Name) : await UpdateAsync(model.Name, model.Id);
        }

        // Deletes a statement type if there are no transactions associated with it.
        public async Task<ServiceResult> DeleteStatementTypeAsync(int id)
        {
            try
            {
                if (await HasLinkedStatementsAsync(id))
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Não é possível excluir este extrato, pois existem registros vinculados a ele. Apague os lançamentos ou transfira-os para outro extrato antes de tentar novamente."
                    };
                }

                var statementType = await _context.StatementTypes.FirstOrDefaultAsync(st => st.Id == id);

                if (statementType == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "O extrato informado não foi encontrado."
                    };
                }

                _context.StatementTypes.Remove(statementType);
                await _context.SaveChangesAsync();

                return new ServiceResult
                {
                    Success = true,
                    Message = "Extrato excluído com sucesso."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete a statement type.");

                return new ServiceResult
                {
                    Success = false,
                    Message = "Ocorreu um erro inesperado e não foi possível concluir a exclusão."
                };
            }
        }

        #endregion

        #region Private Methods

        // Checks if another statement type with the same description already exists.
        private async Task<bool> DoesStatementTypeExistAsync(string name, int id)
        {
            return await _context.StatementTypes.AnyAsync(st => st.Description == name && st.Id != id);
        }

        // Checks if a statement type is currently referenced by any transactions.
        private async Task<bool> HasLinkedStatementsAsync(int id)
        {
            return await _context.Statement.AnyAsync(s => s.StatementTypeId == id);
        }

        // Inserts a new statement type into the database.
        private async Task<ServiceResult> InsertAsync(string name)
        {
            try
            {
                var newStatement = new StatementTypes { Description = name };

                _context.StatementTypes.Add(newStatement);
                await _context.SaveChangesAsync();

                return new ServiceResult
                {
                    Success = true,
                    Message = "Nova opção criada com sucesso!",
                    Id = newStatement.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create a statement type.");

                return new ServiceResult
                {
                    Success = false,
                    Message = "Ocorreu um erro inesperado e não foi possível criar a nova opção."
                };
            }
        }

        // Updates an existing statement type in the database.
        private async Task<ServiceResult> UpdateAsync(string name, int id)
        {
            try
            {
                var existing = await _context.StatementTypes.FirstOrDefaultAsync(st => st.Id == id);

                if (existing == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Registro não encontrado para atualização."
                    };
                }

                existing.Description = name;

                await _context.SaveChangesAsync();

                return new ServiceResult
                {
                    Success = true,
                    Message = "Opção atualizada com sucesso!",
                    Id = existing.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update a statement type.");

                return new ServiceResult
                {
                    Success = false,
                    Message = "Ocorreu um erro inesperado e não foi possível atualizar a opção."
                };
            }
        }

        #endregion
    }
}