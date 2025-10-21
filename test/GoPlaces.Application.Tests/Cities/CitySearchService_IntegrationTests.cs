using System.Threading.Tasks;
using GoPlaces.Cities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace GoPlaces.Tests.Cities
{
    [Collection(GoPlacesTestConsts.CollectionDefinitionName)]
    public class CitySearchService_IntegrationTests
        : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly ICitySearchService _service;
        private readonly IConfiguration _config;

        public CitySearchService_IntegrationTests()
        {
            _service = GetRequiredService<ICitySearchService>();
            _config = GetRequiredService<IConfiguration>();
        }

        [Fact]
        public async Task SearchCitiesAsync_devuelve_resultados_reales()
        {
            (_config["RapidApi:ApiKey"] ?? string.Empty).ShouldNotBeNullOrWhiteSpace();

            var req = new CitySearchRequestDto { PartialName = "Lon" };
            var res = await _service.SearchCitiesAsync(req);

            res.ShouldNotBeNull();
            res.Cities.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task SearchCitiesAsync_maneja_busqueda_sin_resultados()
        {
            var req = new CitySearchRequestDto { PartialName = "123xyz_nonexistent" };
            var res = await _service.SearchCitiesAsync(req);

            res.ShouldNotBeNull();
            res.Cities.ShouldBeEmpty();
        }
    }
}
