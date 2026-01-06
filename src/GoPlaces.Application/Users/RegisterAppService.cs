using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;

namespace GoPlaces.Users
{
    public class RegisterAppService : ApplicationService, IMyRegisterAppService
    {
        private readonly IdentityUserManager _userManager;

        public RegisterAppService(IdentityUserManager userManager)
        {
            _userManager = userManager;
        }

        public async Task RegisterAsync(RegisterInputDto input)
        {
            // 1. Crear el objeto usuario de ABP Identity
            var user = new IdentityUser(
                GuidGenerator.Create(),
                input.UserName,
                input.Email,
                CurrentTenant.Id
            );

            // 2. Intentar guardarlo en la Base de Datos con la contraseña
            var result = await _userManager.CreateAsync(user, input.Password);

            // 3. ¡IMPORTANTE! Si falla (ej: password débil), esto lanza el error al frontend.
            // Si falta esta línea, el backend puede fallar silenciosamente y decir "OK".
            if (!result.Succeeded)
            {
                // Convertimos los errores de Identity a errores amigables de ABP
                throw new Volo.Abp.UserFriendlyException(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // Si llega aquí, ABP hace el "Commit" automático a la base de datos al terminar el método.
        }
    }
}