using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GoPlaces.Cities
{
    public interface ICitySearchService : IApplicationService
    {
        Task<CitySearchResultDto> SearchCitiesAsync(CitySearchRequestDto request);

        // 👇 AQUÍ ESTÁ EL CAMBIO: de int a Guid
        Task<CityDto> GetByIdAsync(Guid id);
    }
}