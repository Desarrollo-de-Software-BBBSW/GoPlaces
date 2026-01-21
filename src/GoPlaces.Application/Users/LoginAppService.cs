using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // 👈 Necesario para SignInManager
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using Volo.Abp;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace GoPlaces.Users
{
    public class LoginAppService : ApplicationService, IMyLoginAppService
    {
        // Usamos SignInManager en lugar de UserManager porque este SÍ crea la cookie de sesión
        private readonly SignInManager<IdentityUser> _signInManager;

        public LoginAppService(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<bool> LoginAsync(LoginInputDto input)
        {
            // 1. Intentar iniciar sesión (CheckPassword + Crear Cookie)
            // El tercer parámetro 'isPersistent: true' mantiene la sesión abierta aunque cierres el navegador
            var result = await _signInManager.PasswordSignInAsync(
                input.UserNameOrEmail,
                input.Password,
                isPersistent: true,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                return true;
            }

            // Si falla, lanzamos error como antes
            throw new UserFriendlyException("Usuario o contraseña incorrectos");
        }
    }
}