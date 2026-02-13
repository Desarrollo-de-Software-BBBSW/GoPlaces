using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoPlaces.Cities;
using GoPlaces.Ratings;
using Moq;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

// ⚠️ NO AGREGUES 'using GoPlaces.Destinations;' para evitar conflictos.

namespace GoPlaces.Tests.Cities
{
    public class CityAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly CityAppService _cityAppService;

        // Mocks
        private readonly Mock<ICitySearchService> _mockCitySearchService;

        // ✅ CORRECCIÓN: Usamos 'GoPlaces.Destinations.Destination' explícitamente
        private readonly Mock<IRepository<GoPlaces.Destinations.Destination, Guid>> _mockDestinationRepository;
        private readonly Mock<IRepository<Rating, Guid>> _mockRatingRepository;

        public CityAppService_Tests()
        {
            _mockCitySearchService = new Mock<ICitySearchService>();

            // ✅ CORRECCIÓN: Inicialización con el nombre completo
            _mockDestinationRepository = new Mock<IRepository<GoPlaces.Destinations.Destination, Guid>>();
            _mockRatingRepository = new Mock<IRepository<Rating, Guid>>();

            _cityAppService = new CityAppService(
                _mockCitySearchService.Object,
                _mockDestinationRepository.Object,
                _mockRatingRepository.Object
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

            // Simulamos que NO existe en base de datos local (devuelve null)
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
    }
}