using System;
using System.Reflection; // 👈 NECESARIO PARA EL TRUCO
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using NSubstitute;
using Volo.Abp.Identity;
using Microsoft.AspNetCore.Identity;
using GoPlaces.Users;
using Volo.Abp;

namespace GoPlaces.Tests.Users
{
    public class LoginAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly IMyLoginAppService _loginAppService;
        private readonly IIdentityUserRepository _fakeUserRepository;

        public LoginAppService_Tests()
        {
            _loginAppService = GetRequiredService<IMyLoginAppService>();
            _fakeUserRepository = GetRequiredService<IIdentityUserRepository>();
        }

        // Método auxiliar para "hackear" la contraseña (Reflexión)
        private void SetPasswordHash(IdentityUser user, string hashedPassword)
        {
            // Buscamos la propiedad "PasswordHash" y forzamos su valor aunque sea privada/protegida
            typeof(IdentityUser)
                .GetProperty("PasswordHash")
                ?.SetValue(user, hashedPassword);
        }

        [Fact]
        public async Task Should_Login_With_Valid_Credentials()
        {
            // 1. ARRANGE
            var password = "Password123!";
            var username = "JuanPerez";

            var fakeUser = new IdentityUser(Guid.NewGuid(), username, "juan@goplaces.com");
            var hasher = new PasswordHasher<IdentityUser>();
            var hashedPass = hasher.HashPassword(fakeUser, password);

            SetPasswordHash(fakeUser, hashedPass);

            // 👇 CORREGIDO: Agregamos Arg.Any<bool>() en el medio
            _fakeUserRepository
                .FindByNormalizedUserNameAsync(
                    Arg.Any<string>(),
                    Arg.Any<bool>(), // <--- ESTE FALTABA
                    Arg.Any<CancellationToken>()
                )
                .Returns(Task.FromResult(fakeUser));

            // 2. ACT
            var input = new LoginInputDto
            {
                UserNameOrEmail = username,
                Password = password
            };

            var result = await _loginAppService.LoginAsync(input);

            // 3. ASSERT
            result.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Fail_With_Wrong_Password()
        {
            // 1. ARRANGE
            var correctPassword = "Password123!";
            var fakeUser = new IdentityUser(Guid.NewGuid(), "JuanPerez", "juan@goplaces.com");

            var hasher = new PasswordHasher<IdentityUser>();
            var hashedPass = hasher.HashPassword(fakeUser, correctPassword);

            SetPasswordHash(fakeUser, hashedPass);

            // 👇 CORREGIDO AQUÍ TAMBIÉN
            _fakeUserRepository
                .FindByNormalizedUserNameAsync(
                    Arg.Any<string>(),
                    Arg.Any<bool>(), // <--- ESTE FALTABA
                    Arg.Any<CancellationToken>()
                )
                .Returns(Task.FromResult(fakeUser));

            // 2. ACT & ASSERT
            await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            {
                await _loginAppService.LoginAsync(new LoginInputDto
                {
                    UserNameOrEmail = "JuanPerez",
                    Password = "WRONG_PASSWORD"
                });
            });
        }
    }
}