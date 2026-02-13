using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.DependencyInjection;

namespace GoPlaces.Users;

[ExposeServices(typeof(IMyLoginAppService))]
public class LoginAppService : GoPlacesAppService, IMyLoginAppService
{
    // Cambiamos UserManager<IdentityUser> por IdentityUserManager (el estándar de ABP)
    protected IdentityUserManager UserManager { get; }

    public LoginAppService(IdentityUserManager userManager)
    {
        UserManager = userManager;
    }

    public virtual async Task<bool> LoginAsync(LoginInputDto input)
    {
        var user = await UserManager.FindByNameAsync(input.UserNameOrEmail)
                   ?? await UserManager.FindByEmailAsync(input.UserNameOrEmail);

        if (user == null)
        {
            throw new UserFriendlyException("Usuario o contraseña incorrectos.");
        }

        // Usamos UserManager para verificar la contraseña en lugar de SignInManager
        // Esto funciona perfectamente en Tests y en Producción
        return await UserManager.CheckPasswordAsync(user, input.Password);
    }
}