using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace GoPlaces.Users
{
    public class MyRegisterAppService : GoPlacesAppService
    {
        private readonly IdentityUserManager _userManager;

        public MyRegisterAppService(IdentityUserManager userManager)
        {
            _userManager = userManager;
        }

        public async Task RegisterCustomUserAsync(string userName, string email, string password)
        {
            // Creamos la entidad del usuario
            var user = new Volo.Abp.Identity.IdentityUser(
                GuidGenerator.Create(),
                userName,
                email,
                CurrentTenant.Id
            );

            // Usamos el manager de ABP para crearlo con password (esto ya lo hashea)
            (await _userManager.CreateAsync(user, password)).CheckErrors();
        }
    }
}