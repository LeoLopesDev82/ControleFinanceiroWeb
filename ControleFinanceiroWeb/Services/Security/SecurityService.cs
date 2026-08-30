using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Models;
using ControleFinanceiroWeb.Models.Entities;

namespace ControleFinanceiroWeb.Services.Security
{
    public class SecurityService : ISecurityService
    {
        private readonly AppDbContext _context;
        private readonly IPinLockout _lockout;
        private readonly ILogger<SecurityService> _logger;
        private readonly PasswordHasher<AppSecurity> _hasher = new();

        public SecurityService(AppDbContext context, IPinLockout lockout, ILogger<SecurityService> logger)
        {
            _context = context;
            _lockout = lockout;
            _logger = logger;
        }

        #region Public Methods

        public async Task<bool> IsPinConfiguredAsync()
        {
            return await _context.AppSecurity.AnyAsync();
        }

        public async Task<ServiceResult> DefinePinAsync(string pin)
        {
            var formatResult = ValidateFormat(pin);

            if (!formatResult.Success)
                return formatResult;

            if (await IsPinConfiguredAsync())
            {
                return new ServiceResult { Success = false, Message = "O PIN já foi definido." };
            }

            try
            {
                var entity = new AppSecurity();

                entity.PinHash = _hasher.HashPassword(entity, pin);

                _context.AppSecurity.Add(entity);
                await _context.SaveChangesAsync();

                return new ServiceResult { Success = true, Message = "PIN definido com sucesso." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store the access PIN.");

                return new ServiceResult { Success = false, Message = "Ocorreu um erro ao gravar o PIN." };
            }
        }

        public async Task<ServiceResult> ValidatePinAsync(string pin)
        {
            if (_lockout.IsLocked())
            {
                return BuildLockedResult();
            }

            var formatResult = ValidateFormat(pin);

            if (!formatResult.Success)
                return formatResult;

            try
            {
                var entity = await _context.AppSecurity.FirstOrDefaultAsync();

                if (entity == null)
                {
                    return new ServiceResult { Success = false, Message = "Nenhum PIN foi definido ainda." };
                }

                var verification = _hasher.VerifyHashedPassword(entity, entity.PinHash, pin);

                if (verification == PasswordVerificationResult.Failed)
                {
                    return RegisterFailedAttempt();
                }

                _lockout.RegisterSuccess();

                return new ServiceResult { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate the access PIN.");

                return new ServiceResult { Success = false, Message = "Ocorreu um erro ao validar o PIN." };
            }
        }

        #endregion

        #region Private Methods

        private ServiceResult ValidateFormat(string pin)
        {
            bool isValid = !string.IsNullOrWhiteSpace(pin)
                && pin.Length == 6
                && pin.All(char.IsDigit);

            if (isValid)
            {
                return new ServiceResult { Success = true };
            }

            return new ServiceResult { Success = false, Message = "O PIN deve ter exatamente 6 dígitos." };
        }

        private ServiceResult RegisterFailedAttempt()
        {
            _lockout.RegisterFailure();

            _logger.LogWarning("Rejected an access attempt with an incorrect PIN.");

            if (_lockout.IsLocked())
            {
                return BuildLockedResult();
            }

            return new ServiceResult { Success = false, Message = "PIN incorreto." };
        }

        private ServiceResult BuildLockedResult()
        {
            int minutes = (int)Math.Ceiling(_lockout.RemainingLockSeconds() / 60d);

            return new ServiceResult
            {
                Success = false,
                Message = $"Muitas tentativas incorretas. Tente novamente em {minutes} minuto(s)."
            };
        }

        #endregion
    }
}
