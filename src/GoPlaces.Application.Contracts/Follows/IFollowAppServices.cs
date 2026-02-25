using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GoPlaces.Follows
{
    public interface IFollowAppService : IApplicationService
    {
        // Método para agregar a favoritos
        Task<SavedDestinationDto> SaveDestinationAsync(SaveOrRemoveInputDto input);
    }
}