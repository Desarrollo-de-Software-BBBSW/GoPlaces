using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace GoPlaces.Ratings
{
    public interface IRatingAppService : IApplicationService
    {
        Task<RatingDto> CreateAsync(CreateRatingDto input);

        // ✅ Corregido: Ahora acepta Guid para coincidir con la base de datos
        Task<ListResultDto<RatingDto>> GetByDestinationAsync(Guid destinationId);

        // ✅ Corregido: Ahora acepta Guid para el chequeo de "ya votó"
        Task<RatingDto?> GetMyForDestinationAsync(Guid destinationId);

        Task<RatingDto> UpdateAsync(Guid id, CreateRatingDto input);
        Task DeleteAsync(Guid id);
    }
}