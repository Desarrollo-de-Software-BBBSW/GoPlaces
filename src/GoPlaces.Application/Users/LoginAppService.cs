using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using Volo.Abp;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace GoPlaces.Users
{
    // Asegúrate de que la interfaz (IMyLoginAppService) sea la correcta según tu proyecto
    public class LoginAppService : ApplicationService, IMyLoginAppService
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager; // 👈 1. AGREGADO: Declaramos la variable

        // 2. MODIFICADO: Inyectamos UserManager en el constructor
        public LoginAppService(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager) // 👈 Pedimos la herramienta aquí
        {
            _signInManager = signInManager;
            _userManager = userManager; // 👈 Guardamos la herramienta para usarla abajo
        }

        public async Task<bool> LoginAsync(LoginInputDto input)
        {
            // 1. Ahora sí podemos usar _userManager para buscar al usuario
            var user = await _userManager.FindByNameAsync(input.UserNameOrEmail)
                       ?? await _userManager.FindByEmailAsync(input.UserNameOrEmail);

            if (user == null)
            {
                throw new UserFriendlyException("El usuario o contraseña son incorrectos.");
            }

            // 2. Verificamos la contraseña SIN crear cookies (evita el error de OpenIddict)
            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                input.Password,
                lockoutOnFailure: false
            );

            if (!result.Succeeded)
            {
                throw new UserFriendlyException("El usuario o contraseña son incorrectos.");
            }

            return true;
        }
    }
}