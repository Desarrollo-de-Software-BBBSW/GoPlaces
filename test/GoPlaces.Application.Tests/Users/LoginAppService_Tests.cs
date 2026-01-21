using System.Threading.Tasks;
using Xunit;
using Shouldly;
using NSubstitute;
using Volo.Abp.Identity;
using Microsoft.AspNetCore.Identity;
using GoPlaces.Users;
using Volo.Abp;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace GoPlaces.Tests.Users
{
    public class LoginAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly IMyLoginAppService _loginAppService;

        // Ahora necesitamos controlar al SignInManager, no al Repositorio
        private readonly SignInManager<IdentityUser> _fakeSignInManager;

        public LoginAppService_Tests()
        {
            _loginAppService = GetRequiredService<IMyLoginAppService>();
            _fakeSignInManager = GetRequiredService<SignInManager<IdentityUser>>();
        }

        [Fact]
        public async Task Should_Login_With_Valid_Credentials()
        {
            // 1. ARRANGE
            var username = "JuanPerez";
            var password = "Password123!";

            // Simulamos que SignInManager dice "¡ÉXITO!"
            // Nota: No necesitamos crear usuarios reales ni hashes, solo simular la respuesta
            _fakeSignInManager
                .PasswordSignInAsync(username, password, true, false)
                .Returns(Task.FromResult(SignInResult.Success));

            // 2. ACT
            var input = new LoginInputDto
            {
                UserNameOrEmail = username,
                Password = password
            };

            var result = await _loginAppService.LoginAsync(input);

            // 3. ASSERT
            result.ShouldBeTrue();

            // Verificamos que el servicio haya llamado al método correcto
            await _fakeSignInManager.Received(1)
                .PasswordSignInAsync(username, password, true, false);
        }

        [Fact]
        public async Task Should_Fail_With_Wrong_Password()
        {
            // 1. ARRANGE
            var username = "JuanPerez";
            var wrongPassword = "WrongPassword";

            // Simulamos que SignInManager dice "FALLÓ"
            _fakeSignInManager
                .PasswordSignInAsync(username, wrongPassword, true, false)
                .Returns(Task.FromResult(SignInResult.Failed));

            // 2. ACT & ASSERT
            await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            {
                await _loginAppService.LoginAsync(new LoginInputDto
                {
                    UserNameOrEmail = username,
                    Password = wrongPassword
                });
            });
        }
    }
}