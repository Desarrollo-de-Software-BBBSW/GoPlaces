using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.DependencyInjection;

namespace GoPlaces.Users;

[ExposeServices(typeof(RegisterAppService),typeof(IMyRegisterAppService))]
public class RegisterAppService : GoPlacesAppService, IMyRegisterAppService
{
    private readonly IdentityUserManager _userManager;

    public RegisterAppService(IdentityUserManager userManager) => _userManager = userManager;

    public virtual async Task RegisterAsync(RegisterInputDto input)
    {
        var user = new Volo.Abp.Identity.IdentityUser(
            GuidGenerator.Create(),
            input.UserName,
            input.Email,
            CurrentTenant.Id
        );

        var result = await _userManager.CreateAsync(user, input.Password);

        // ✅ Validación manual: Infalible contra errores de extensión CS1061
        if (!result.Succeeded)
        {
            throw new UserFriendlyException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}