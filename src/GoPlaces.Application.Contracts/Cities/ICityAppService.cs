using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using GoPlaces.Destinations;

namespace GoPlaces.Cities
{
    // Hereda de IApplicationService para que ABP la reconozca
    public interface ICityAppService : IApplicationService
    {
        Task<CitySearchResultDto> SearchCitiesAsync(CitySearchRequestDto request);
    }

}
