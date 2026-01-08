using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity; // Necesario para IdentityUserManager

namespace GoPlaces.Users
{
    public class LoginAppService : ApplicationService, IMyLoginAppService
    {
        private readonly IdentityUserManager _userManager;

        public LoginAppService(IdentityUserManager userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> LoginAsync(LoginInputDto input)
        {
            // 1. Buscar usuario por Nombre o Email
            var user = await _userManager.FindByNameAsync(input.UserNameOrEmail)
                       ?? await _userManager.FindByEmailAsync(input.UserNameOrEmail);

            if (user == null)
            {
                // Por seguridad, no decimos "Usuario no encontrado", sino "Credenciales inválidas"
                throw new UserFriendlyException("Usuario o contraseña incorrectos");
            }

            // 2. Verificar la contraseña (ABP se encarga del Hash)
            var checkPassword = await _userManager.CheckPasswordAsync(user, input.Password);

            if (!checkPassword)
            {
                throw new UserFriendlyException("Usuario o contraseña incorrectos");
            }

            // 3. Si llegamos aquí, es válido
            return true;
        }
    }
}