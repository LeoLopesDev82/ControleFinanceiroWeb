using System;
using System.Linq;
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
        private readonly ILogger<SecurityService> _logger;
        private readonly PasswordHasher<AppSecurity> _hasher = new();

        public SecurityService(AppDbContext context, ILogger<SecurityService> logger)
        {
            _context = context;
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
            try
            {
                var entity = await _context.AppSecurity.FirstOrDefaultAsync();

                if (entity == null)
                {
                    return new ServiceResult { Success = false, Message = "Nenhum PIN foi definido ainda." };
                }

                var remaining = RemainingLock(entity);

                if (remaining.HasValue)
                {
                    return BuildLockedResult(remaining.Value);
                }

                var formatResult = ValidateFormat(pin);

                if (!formatResult.Success)
                    return formatResult;

                var verification = _hasher.VerifyHashedPassword(entity, entity.PinHash, pin);

                if (verification == PasswordVerificationResult.Failed)
                {
                    return await RegisterFailedAttemptAsync(entity);
                }

                await ClearAttemptsAsync(entity);

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

        private static TimeSpan? RemainingLock(AppSecurity entity)
        {
            if (!entity.LockedUntil.HasValue)
                return null;

            var remaining = entity.LockedUntil.Value - DateTime.UtcNow;

            return remaining > TimeSpan.Zero ? remaining : null;
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

        private async Task<ServiceResult> RegisterFailedAttemptAsync(AppSecurity entity)
        {
            entity.FailedAttempts++;

            var lockFor = PinThrottle.LockFor(entity.FailedAttempts);

            entity.LockedUntil = lockFor.HasValue ? DateTime.UtcNow.Add(lockFor.Value) : null;

            await _context.SaveChangesAsync();

            _logger.LogWarning("Rejected an access attempt with an incorrect PIN. Consecutive failures: {FailedAttempts}.", entity.FailedAttempts);

            if (lockFor.HasValue)
            {
                return BuildLockedResult(lockFor.Value);
            }

            return new ServiceResult { Success = false, Message = "PIN incorreto." };
        }

        private async Task ClearAttemptsAsync(AppSecurity entity)
        {
            if (entity.FailedAttempts == 0 && !entity.LockedUntil.HasValue)
                return;

            entity.FailedAttempts = 0;
            entity.LockedUntil = null;

            await _context.SaveChangesAsync();
        }

        private static ServiceResult BuildLockedResult(TimeSpan remaining)
        {
            return new ServiceResult
            {
                Success = false,
                Message = $"Muitas tentativas incorretas. Tente novamente em {PinThrottle.DescribeWait(remaining)}."
            };
        }

        #endregion
    }
}
