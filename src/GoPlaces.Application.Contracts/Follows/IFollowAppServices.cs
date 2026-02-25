using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GoPlaces.Follows
{
    public interface IFollowAppService : IApplicationService
    {
        // Método para agregar a favoritos
        Task<SavedDestinationDto> SaveDestinationAsync(SaveOrRemoveInputDto input);

        // Método para eliminar destino de favoritos
        Task RemoveDestinationAsync(SaveOrRemoveInputDto input);

        // 👇 NUEVO MÉTODO: Consultar lista personal
        Task<List<SavedDestinationDto>> GetMyFavoritesAsync();
    }
}