using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GoPlaces.Cities;
using GoPlaces.ExternalApiMetrics;
using GoPlaces.Ratings;
using Moq;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace GoPlaces.Tests.Cities
{
    public class CityAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly CityAppService _cityAppService;

        // Mocks
        private readonly Mock<ICitySearchService> _mockCitySearchService;
        private readonly Mock<IRepository<GoPlaces.Destinations.Destination, Guid>> _mockDestinationRepository;
        private readonly Mock<IRepository<Rating, Guid>> _mockRatingRepository;
        private readonly Mock<IRepository<ExternalApiCall, Guid>> _mockExternalApiCallRepository;

        public CityAppService_Tests()
        {
            _mockCitySearchService = new Mock<ICitySearchService>();
            _mockDestinationRepository = new Mock<IRepository<GoPlaces.Destinations.Destination, Guid>>();
            _mockRatingRepository = new Mock<IRepository<Rating, Guid>>();
            _mockExternalApiCallRepository = new Mock<IRepository<ExternalApiCall, Guid>>();

            _mockExternalApiCallRepository
                .Setup(r => r.InsertAsync(It.IsAny<ExternalApiCall>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ExternalApiCall e, bool _, CancellationToken __) => e);

            _cityAppService = new CityAppService(
                _mockCitySearchService.Object,
                _mockDestinationRepository.Object,
                _mockRatingRepository.Object,
                _mockExternalApiCallRepository.Object,
                new CitySearchDomainService()
            );
        }

        [Fact]
        public async Task SearchCitiesAsync_devuelve_resultados_del_servicio()
        {
            // ARRANGE
            _mockCitySearchService
                .Setup(s => s.SearchCitiesAsync(It.Is<CitySearchRequestDto>(r => r.PartialName == "Lon")))
                .ReturnsAsync(new CitySearchResultDto
                {
                    Cities = new List<CityDto>
                    {
                        new() { Id = Guid.NewGuid(), Name = "London", Country = "United Kingdom" }
                    }
                });

            // ACT
            var result = await _cityAppService.SearchCitiesAsync(new CitySearchRequestDto { PartialName = "Lon" });

            // ASSERT
            result.ShouldNotBeNull();
            result.Cities.Count.ShouldBe(1);
            result.Cities[0].Name.ShouldBe("London");
        }

        [Fact]
        public async Task GetAsync_devuelve_ciudad_por_id_desde_servicio_externo()
        {
            // 1. ARRANGE
            var cityId = Guid.NewGuid();
            var expectedCity = new CityDto { Id = cityId, Name = "Paris", Country = "France" };

            // Simulamos que NO existe en base de datos local
            _mockDestinationRepository
                .Setup(r => r.FindAsync(cityId, It.IsAny<bool>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync((GoPlaces.Destinations.Destination)null);

            // Simulamos que SÍ existe en API externa
            _mockCitySearchService
                .Setup(s => s.GetByIdAsync(cityId))
                .ReturnsAsync(expectedCity);

            // 2. ACT
            var result = await _cityAppService.GetAsync(cityId);

            // 3. ASSERT
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Paris");
        }

        [Fact]
        public async Task SearchCitiesAsync_Should_Return_Empty_List_When_No_Results()
        {
            // ARRANGE
            _mockCitySearchService
                .Setup(s => s.SearchCitiesAsync(It.Is<CitySearchRequestDto>(r => r.PartialName == "XYZ")))
                .ReturnsAsync(new CitySearchResultDto
                {
                    Cities = new List<CityDto>()
                });

            // ACT
            var result = await _cityAppService.SearchCitiesAsync(new CitySearchRequestDto { PartialName = "XYZ" });

            // ASSERT
            result.ShouldNotBeNull();
            result.Cities.ShouldBeEmpty();
        }

        [Fact]
        public async Task GetListAsync_envia_filtros_correctamente_al_servicio_de_busqueda()
        {
            // ARRANGE
            var request = new CitySearchRequestDto
            {
                PartialName = "Bue",
                CountryCode = "AR",
                RegionId = "BA",
                MinPopulation = 500000
            };

            _mockCitySearchService
                .Setup(s => s.SearchCitiesAsync(It.Is<CitySearchRequestDto>(r =>
                    r.PartialName == "Bue" &&
                    r.CountryCode == "AR" &&
                    r.RegionId == "BA" &&
                    r.MinPopulation == 500000)))
                .ReturnsAsync(new CitySearchResultDto
                {
                    Cities = new List<CityDto>
                    {
                        new() { Id = Guid.NewGuid(), Name = "Buenos Aires", Country = "Argentina" }
                    }
                });

            // ACT
            var result = await _cityAppService.GetListAsync(request);

            // ASSERT
            result.ShouldNotBeNull();
            result.Cities.Count.ShouldBe(1);
            result.Cities[0].Name.ShouldBe("Buenos Aires");

            // Verificar que se llamó al servicio externo con los filtros correctos
            _mockCitySearchService.Verify(
                s => s.SearchCitiesAsync(It.Is<CitySearchRequestDto>(r =>
                    r.CountryCode == "AR" &&
                    r.RegionId == "BA" &&
                    r.MinPopulation == 500000)),
                Times.Once);

            // Verificar que se registró la métrica de la llamada a la API
            _mockExternalApiCallRepository.Verify(
                r => r.InsertAsync(It.IsAny<ExternalApiCall>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetListAsync_lanza_excepcion_si_minPopulation_es_negativa()
        {
            // ARRANGE
            var request = new CitySearchRequestDto
            {
                PartialName = "Buenos",
                MinPopulation = -1
            };

            // ACT & ASSERT
            await Should.ThrowAsync<Volo.Abp.UserFriendlyException>(
                () => _cityAppService.GetListAsync(request));
        }
    }
}