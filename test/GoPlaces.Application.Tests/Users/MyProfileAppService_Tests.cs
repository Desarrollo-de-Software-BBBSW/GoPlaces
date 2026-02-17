using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace GoPlaces.Users;

public class MyProfileAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
{
    private readonly IMyProfileAppService _profileAppService;
    private readonly IdentityUserManager _userManager;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public MyProfileAppService_Tests()
    {
        _profileAppService = GetRequiredService<IMyProfileAppService>();
        _userManager = GetRequiredService<IdentityUserManager>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private IDisposable CambiarUsuario(Guid userId, string email)
    {
        var claims = new List<Claim>
        {
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.Email, email),
            new Claim(AbpClaimTypes.UserName, "testuser")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        return _currentPrincipalAccessor.Change(principal);
    }

    [Fact]
    public async Task GetAsync_Should_Return_Current_User_Profile()
    {
        var userId = Guid.NewGuid();
        var email = "test@test.com";

        using (CambiarUsuario(userId, email))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var user = new IdentityUser(userId, "testuser", email);
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded) throw new UserFriendlyException("Error al crear usuario");
            });

            var result = await _profileAppService.GetAsync();
            result.Id.ShouldBe(userId);
        }
    }

    [Fact]
    public async Task UpdateAsync_Should_Save_Changes_To_Repository()
    {
        var userId = Guid.NewGuid();
        var email = "old@test.com";

        using (CambiarUsuario(userId, email))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var user = new IdentityUser(userId, "olduser", email);
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded) throw new UserFriendlyException("Error al crear usuario");
            });

            var input = new UserProfileDto
            {
                Name = "NewName",
                Email = "new@test.com"
            };

            await _profileAppService.UpdateAsync(input);

            var updatedUser = await _userManager.GetByIdAsync(userId);
            updatedUser.Name.ShouldBe("NewName");
        }
    }

    [Fact]
    public async Task ChangePasswordAsync_Should_Work_When_CurrentPassword_Is_Correct()
    {
        var userId = Guid.NewGuid();
        var currentPass = "OldPass.123!";
        var newPass = "NewPass.123!";

        using (CambiarUsuario(userId, "pass@test.com"))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var user = new IdentityUser(userId, "passuser", "pass@test.com");
                var result = await _userManager.CreateAsync(user, currentPass);
                if (!result.Succeeded) throw new UserFriendlyException("Error al crear usuario");
            });

            await _profileAppService.ChangePasswordAsync(new ChangePasswordInputDto
            {
                CurrentPassword = currentPass,
                NewPassword = newPass
            });

            var userAfter = await _userManager.GetByIdAsync(userId);
            (await _userManager.CheckPasswordAsync(userAfter, newPass)).ShouldBeTrue();
        }
    }

    // 👇👇👇 ESTA ES LA PRUEBA QUE FALTABA 👇👇👇
    [Fact]
    public async Task ChangePasswordAsync_Should_Throw_Exception_When_CurrentPassword_Is_Wrong()
    {
        var userId = Guid.NewGuid();
        var realPass = "RealPass.123!";
        var wrongPass = "Wrong.123!"; // Contraseña incorrecta intencional

        using (CambiarUsuario(userId, "wrongpass@test.com"))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var user = new IdentityUser(userId, "wrongpassuser", "wrongpass@test.com");
                // Creamos el usuario con la contraseña REAL
                var result = await _userManager.CreateAsync(user, realPass);
                if (!result.Succeeded) throw new UserFriendlyException("Error al crear usuario");
            });

            // Intentamos cambiar la contraseña enviando la INCORRECTA como actual
            // Esto debe lanzar AbpIdentityResultException (porque Identity devuelve Failed)
            await Assert.ThrowsAsync<AbpIdentityResultException>(async () =>
            {
                await _profileAppService.ChangePasswordAsync(new ChangePasswordInputDto
                {
                    CurrentPassword = wrongPass,
                    NewPassword = "NewPassword.123!"
                });
            });
        }
    }

    [Fact]
    public async Task DeleteAsync_Should_Soft_Delete_Current_User()
    {
        var userId = Guid.NewGuid();

        using (CambiarUsuario(userId, "del@test.com"))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var user = new IdentityUser(userId, "deluser", "del@test.com");
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded) throw new UserFriendlyException("Error al crear usuario");
            });

            await _profileAppService.DeleteAsync();

            var deletedUser = await _userManager.FindByIdAsync(userId.ToString());
            deletedUser.ShouldBeNull();
        }
    }
}