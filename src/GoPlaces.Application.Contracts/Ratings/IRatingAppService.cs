using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;

namespace GoPlaces.Ratings;

public interface IRatingAppService : IApplicationService
{
    Task<RatingDto> CreateAsync(CreateRatingDto input);

    // Lista de ratings del destino (por el filtro global, verás solo los tuyos)
    Task<ListResultDto<RatingDto>> GetByDestinationAsync(Guid destinationId);

    // Tu rating (único) para un destino (si existe)
    Task<RatingDto?> GetMyForDestinationAsync(Guid destinationId);
}
