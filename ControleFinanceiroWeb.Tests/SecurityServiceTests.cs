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
        public async Task ValidatePinAsync_ShouldNotLock_OnTheFirstTwoMistakes()
        {
            using var context = InMemoryDatabase.Create();

            var service = CreateService(context);

            await service.DefinePinAsync("246810");

            for (int attempt = 0; attempt < 2; attempt++)
            {
                var wrong = await service.ValidatePinAsync("000000");

                Assert.Equal("PIN incorreto.", wrong.Message);
            }

            Assert.True((await service.ValidatePinAsync("246810")).Success);
        }

        [Fact]
        public async Task ValidatePinAsync_ShouldLock_OnTheThirdMistake()
        {
            using var context = InMemoryDatabase.Create();

            var service = CreateService(context);

            await service.DefinePinAsync("246810");

            await service.ValidatePinAsync("000000");
            await service.ValidatePinAsync("000000");

            var third = await service.ValidatePinAsync("000000");

            Assert.False(third.Success);
            Assert.Contains("30 segundos", third.Message);

            var correct = await service.ValidatePinAsync("246810");

            Assert.False(correct.Success);
            Assert.Contains("Muitas tentativas", correct.Message);
        }

        [Theory]
        [InlineData(3, "30 segundos")]
        [InlineData(4, "2 minutos")]
        [InlineData(5, "5 minutos")]
        [InlineData(6, "15 minutos")]
        [InlineData(9, "15 minutos")]
        public async Task ValidatePinAsync_ShouldStretchTheWait_AsMistakesPileUp(int attempts, string expectedWait)
        {
            using var context = InMemoryDatabase.Create();

            var service = CreateService(context);

            await service.DefinePinAsync("246810");

            string message = string.Empty;

            for (int attempt = 0; attempt < attempts; attempt++)
            {
                ReleaseLock(context);

                message = (await service.ValidatePinAsync("000000")).Message;
            }

            Assert.Contains(expectedWait, message);
        }

        [Fact]
        public async Task ValidatePinAsync_ShouldForgetTheMistakes_OnceTheCorrectPinIsAccepted()
        {
            using var context = InMemoryDatabase.Create();

            var service = CreateService(context);

            await service.DefinePinAsync("246810");

            await service.ValidatePinAsync("000000");
            await service.ValidatePinAsync("000000");
            await service.ValidatePinAsync("246810");

            var afterSuccess = await service.ValidatePinAsync("000000");

            Assert.Equal("PIN incorreto.", afterSuccess.Message);
            Assert.Equal(1, context.AppSecurity.Single().FailedAttempts);
        }

        [Fact]
        public async Task ValidatePinAsync_ShouldKeepTheLock_AcrossServiceInstances()
        {
            using var context = InMemoryDatabase.Create();

            await CreateService(context).DefinePinAsync("246810");

            for (int attempt = 0; attempt < 3; attempt++)
            {
                await CreateService(context).ValidatePinAsync("000000");
            }

            var result = await CreateService(context).ValidatePinAsync("246810");

            Assert.False(result.Success);
            Assert.Contains("Muitas tentativas", result.Message);
        }

        #endregion

        #region Private Methods

        private static SecurityService CreateService(AppDbContext context)
        {
            return new SecurityService(context, NullLogger<SecurityService>.Instance);
        }

        private static void ReleaseLock(AppDbContext context)
        {
            var entity = context.AppSecurity.SingleOrDefault();

            if (entity == null)
                return;

            entity.LockedUntil = null;

            context.SaveChanges();
        }

        #endregion
    }
}
