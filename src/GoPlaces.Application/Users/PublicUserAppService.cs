using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Identity;

namespace GoPlaces.Users
{
    [Authorize] // Solo usuarios logueados pueden ver perfiles
    public class PublicUserAppService : ApplicationService, IPublicUserAppService
    {
        private readonly IdentityUserManager _userManager;

        public PublicUserAppService(IdentityUserManager userManager)
        {
            _userManager = userManager;
        }

        public async Task<PublicUserProfileDto> GetByUserNameAsync(string userName)
        {
            // 1. Buscamos el usuario por su UserName
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                throw new UserFriendlyException($"El usuario '{userName}' no existe.");
            }

            // 2. Mapeamos MANUALMENTE solo los datos seguros.
            // (No usamos AutoMapper aquí para estar 100% seguros de no filtrar el email por error)
            return new PublicUserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Name = user.Name,
                Surname = user.Surname,

                // Extraemos las propiedades extra con seguridad
                PhotoUrl = user.ExtraProperties.ContainsKey("PhotoUrl") ? user.GetProperty<string>("PhotoUrl") : null,
                Preferences = user.ExtraProperties.ContainsKey("Preferences") ? user.GetProperty<string>("Preferences") : null
            };
        }
    }
}