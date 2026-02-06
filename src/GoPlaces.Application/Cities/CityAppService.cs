using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;

namespace GoPlaces.Cities
{
    public class CityAppService : ApplicationService
    {
        private readonly ICitySearchService _citySearchService;

        // Un único constructor con sus dependencias
        public CityAppService(ICitySearchService citySearchService)
        {
            _citySearchService = citySearchService;
        }

        // Un único método público que cumple su propósito
        public async Task<CitySearchResultDto> SearchCitiesAsync(CitySearchRequestDto request)
        {
            return await _citySearchService.SearchCitiesAsync(request);
        }
        public async Task<CityDto> GetAsync(int id)
        {
            // Simplemente llamamos al servicio interno pidiendo por ID
            return await _citySearchService.GetByIdAsync(id);
        }
    }

}
