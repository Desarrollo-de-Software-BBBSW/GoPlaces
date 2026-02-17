using GoPlaces.Users;
using Microsoft.AspNetCore.Identity;
using Shouldly;
using System.Threading.Tasks;
using Volo.Abp.Guids;
using Volo.Abp.Uow;
using Xunit;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using IdentityUserManager = Volo.Abp.Identity.IdentityUserManager;

namespace GoPlaces.Tests.Users
{
    public class LoginAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly IdentityUserManager _userManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IGuidGenerator _guidGenerator;

        public LoginAppService_Tests()
        {
            _userManager = GetRequiredService<IdentityUserManager>();
            _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
            _guidGenerator = GetRequiredService<IGuidGenerator>();
        }

        [Fact]
        public async Task Should_Login_With_Valid_Credentials()
        {
            var username = "login_master";
            var password = "1q2w3E*Password";

            await WithUnitOfWorkAsync(async () =>
            {
                var user = new IdentityUser(_guidGenerator.Create(), username, "master@test.com");
                user.SetEmailConfirmed(true);

                await _userManager.UpdateSecurityStampAsync(user);
                (await _userManager.CreateAsync(user, password)).CheckErrors();

                await _unitOfWorkManager.Current.SaveChangesAsync();

                var loginService = GetRequiredService<IMyLoginAppService>();
                var result = await loginService.LoginAsync(new LoginInputDto
                {
                    UserNameOrEmail = username,
                    Password = password
                });

                result.ShouldBeTrue();
            });
        }

        // 👇👇👇 CORRECCIÓN AQUÍ 👇👇👇
        // Tu servicio devuelve 'false' si la password está mal, NO lanza excepción.
        [Fact]
        public async Task Should_Fail_With_Wrong_Password()
        {
            var username = "fail_user";
            var correctPassword = "CorrectPassword123!";

            await WithUnitOfWorkAsync(async () =>
            {
                // 1. Crear usuario con contraseña correcta
                var user = new IdentityUser(_guidGenerator.Create(), username, "fail@test.com");
                user.SetEmailConfirmed(true);

                await _userManager.UpdateSecurityStampAsync(user);
                (await _userManager.CreateAsync(user, correctPassword)).CheckErrors();

                await _unitOfWorkManager.Current.SaveChangesAsync();

                var loginService = GetRequiredService<IMyLoginAppService>();

                // 2. Intentar loguearse con contraseña INCORRECTA
                var result = await loginService.LoginAsync(new LoginInputDto
                {
                    UserNameOrEmail = username,
                    Password = "WrongPassword!" // ❌
                });

                // 3. El resultado debe ser FALSE (Login fallido)
                result.ShouldBeFalse();
            });
        }
    }
}