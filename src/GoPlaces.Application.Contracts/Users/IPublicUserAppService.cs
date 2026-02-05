using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GoPlaces.Users
{
    public interface IPublicUserAppService : IApplicationService
    {
        // Buscamos por string (username) en lugar de Guid
        Task<PublicUserProfileDto> GetByUserNameAsync(string userName);
    }
}