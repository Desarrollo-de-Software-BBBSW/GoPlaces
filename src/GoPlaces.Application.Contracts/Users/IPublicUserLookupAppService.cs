using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GoPlaces.Users;

/// <summary>
/// Contrato para la búsqueda de perfiles públicos de usuario.
/// </summary>
public interface IPublicUserLookupAppService : IApplicationService
{
    /// <summary>
    /// Obtiene el perfil público de un usuario mediante su nombre de usuario.
    /// </summary>
    /// <param name="userName">Nombre de usuario (ej: 'pedro123')</param>
    /// <returns>DTO con los datos públicos del usuario</returns>
    Task<PublicUserProfileDto> GetByUserNameAsync(string userName);
}