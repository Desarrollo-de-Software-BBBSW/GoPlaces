using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GoPlaces.Users
{
    public interface IMyProfileAppService : IApplicationService
    {
        // Obtener mis datos actuales
        Task<UserProfileDto> GetAsync();

        // Guardar cambios
        Task UpdateAsync(UserProfileDto input);

        //Cambiar contraseña
        Task ChangePasswordAsync(ChangePasswordInputDto input);
    }
}