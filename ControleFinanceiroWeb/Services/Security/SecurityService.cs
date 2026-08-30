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
                var entity = new AppSecurity { SecurityStamp = NewSecurityStamp() };

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

        public async Task<ServiceResult> ChangePinAsync(string currentPin, string newPin)
        {
            var currentPinResult = await ValidatePinAsync(currentPin);

            if (!currentPinResult.Success)
            {
                return currentPinResult;
            }

            var formatResult = ValidateFormat(newPin);

            if (!formatResult.Success)
                return formatResult;

            if (currentPin == newPin)
            {
                return new ServiceResult { Success = false, Message = "O novo PIN deve ser diferente do atual." };
            }

            try
            {
                var entity = await _context.AppSecurity.FirstOrDefaultAsync();

                if (entity == null)
                {
                    return new ServiceResult { Success = false, Message = "Nenhum PIN foi definido ainda." };
                }

                entity.PinHash = _hasher.HashPassword(entity, newPin);
                entity.SecurityStamp = NewSecurityStamp();

                await _context.SaveChangesAsync();

                _logger.LogInformation("The access PIN was changed and the previous sessions were invalidated.");

                return new ServiceResult { Success = true, Message = "PIN alterado com sucesso." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change the access PIN.");

                return new ServiceResult { Success = false, Message = "Ocorreu um erro ao alterar o PIN." };
            }
        }

        public async Task<string?> GetSecurityStampAsync()
        {
            return await _context.AppSecurity
                .Select(a => a.SecurityStamp)
                .FirstOrDefaultAsync();
        }

        #endregion

        #region Private Methods

        private static string NewSecurityStamp()
        {
            return Guid.NewGuid().ToString("N");
        }

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
