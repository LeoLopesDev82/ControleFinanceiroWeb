using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using ControleFinanceiroWeb.Data;
using ControleFinanceiroWeb.Services.Security;

namespace ControleFinanceiroWeb.Tests
{
    public class SecurityServiceTests
    {
        #region Public Methods

        [Fact]
        public async Task DefinePinAsync_ShouldStoreAHash_RatherThanThePinItself()
        {
            using var context = InMemoryDatabase.Create();

            var result = await CreateService(context).DefinePinAsync("246810");

            Assert.True(result.Success);

            var stored = context.AppSecurity.Single();

            Assert.DoesNotContain("246810", stored.PinHash);
            Assert.NotEmpty(stored.SecurityStamp);
        }

        [Fact]
        public async Task DefinePinAsync_ShouldRefuseAPin_ThatIsNotSixDigits()
        {
            using var context = InMemoryDatabase.Create();

            var service = CreateService(context);

            Assert.False((await service.DefinePinAsync("1234")).Success);
            Assert.False((await service.DefinePinAsync("12345a")).Success);
            Assert.Empty(context.AppSecurity);
        }

        [Fact]
        public async Task ValidatePinAsync_ShouldAcceptTheDefinedPin_AndRejectAnother()
        {
            using var context = InMemoryDatabase.Create();

            var service = CreateService(context);

            await service.DefinePinAsync("246810");

            Assert.True((await service.ValidatePinAsync("246810")).Success);
            Assert.False((await service.ValidatePinAsync("111111")).Success);
        }

        [Fact]
        public async Task ChangePinAsync_ShouldRefuse_WhenTheCurrentPinIsWrong()
        {
            using var context = InMemoryDatabase.Create();

            var service = CreateService(context);

            await service.DefinePinAsync("246810");

            var result = await service.ChangePinAsync("999999", "135790");

            Assert.False(result.Success);
            Assert.True((await service.ValidatePinAsync("246810")).Success);
        }

        [Fact]
        public async Task ChangePinAsync_ShouldRefuse_WhenTheNewPinRepeatsTheCurrentOne()
        {
            using var context = InMemoryDatabase.Create();

            var service = CreateService(context);

            await service.DefinePinAsync("246810");

            var result = await service.ChangePinAsync("246810", "246810");

            Assert.False(result.Success);
            Assert.Contains("diferente do atual", result.Message);
        }

        [Fact]
        public async Task ChangePinAsync_ShouldReplaceThePin_AndRotateTheSecurityStamp()
        {
            using var context = InMemoryDatabase.Create();

            var service = CreateService(context);

            await service.DefinePinAsync("246810");

            var stampBefore = await service.GetSecurityStampAsync();

            var result = await service.ChangePinAsync("246810", "135790");

            Assert.True(result.Success);
            Assert.True((await service.ValidatePinAsync("135790")).Success);
            Assert.False((await service.ValidatePinAsync("246810")).Success);
            Assert.NotEqual(stampBefore, await service.GetSecurityStampAsync());
        }

        [Fact]
        public async Task ValidatePinAsync_ShouldLockOut_AfterFiveWrongAttempts()
        {
            using var context = InMemoryDatabase.Create();

            var service = CreateService(context);

            await service.DefinePinAsync("246810");

            for (int attempt = 0; attempt < 5; attempt++)
            {
                await service.ValidatePinAsync("000000");
            }

            var result = await service.ValidatePinAsync("246810");

            Assert.False(result.Success);
            Assert.Contains("Muitas tentativas", result.Message);
        }

        #endregion

        #region Private Methods

        private static SecurityService CreateService(AppDbContext context)
        {
            return new SecurityService(context, new PinLockout(), NullLogger<SecurityService>.Instance);
        }

        #endregion
    }
}
