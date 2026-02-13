using System;
using System.Collections.Generic;
using System.Security.Claims; // 👈 Necesario para crear la identidad manualmente
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims; // 👈 Necesario para ICurrentPrincipalAccessor
using Xunit;
using Microsoft.Extensions.DependencyInjection;
// 👇 Resolvemos la ambigüedad (IdentityUser)
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace GoPlaces.Users;

public class MyProfileAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
{
    private readonly IMyProfileAppService _profileAppService;
    private readonly IdentityUserManager _userManager;
    // En lugar de ICurrentUser, usamos el Accessor de bajo nivel
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public MyProfileAppService_Tests()
    {
        _profileAppService = GetRequiredService<IMyProfileAppService>();
        _userManager = GetRequiredService<IdentityUserManager>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    // 👇 MÉTODO AYUDANTE: Esto hace lo mismo que .Change() pero manualmente
    private IDisposable CambiarUsuario(Guid userId, string email)
    {
        var claims = new List<Claim>
        {
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.Email, email),
            new Claim(AbpClaimTypes.UserName, "testuser") // Opcional
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

        // Usamos nuestro método manual
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