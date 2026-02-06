using System.Collections.Generic;
using System.Threading.Tasks;
using GoPlaces.Cities;
using Moq;
using Shouldly;
using Xunit;

namespace GoPlaces.Tests.Cities
{
    public class CityAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly CityAppService _cityAppService;
        private readonly Mock<ICitySearchService> _mockCitySearchService;

        public CityAppService_Tests()
        {
            // Inicializamos el Mock y el Servicio
            _mockCitySearchService = new Mock<ICitySearchService>();
            _cityAppService = new CityAppService(_mockCitySearchService.Object);
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
                        new() { Id = 1, Name = "London", Country = "United Kingdom" }
                    }
                });

            // ACT
            var result = await _cityAppService.SearchCitiesAsync(new CitySearchRequestDto { PartialName = "Lon" });

            // ASSERT
            result.ShouldNotBeNull();
            result.Cities.Count.ShouldBe(1);
            result.Cities[0].Name.ShouldBe("London");

            _mockCitySearchService.Verify(
                s => s.SearchCitiesAsync(It.IsAny<CitySearchRequestDto>()),
                Times.Once);
        }

        [Fact]
        public async Task SearchCitiesAsync_cuando_no_hay_resultados_devuelve_lista_vacia()
        {
            // ARRANGE
            _mockCitySearchService
                .Setup(s => s.SearchCitiesAsync(It.Is<CitySearchRequestDto>(r => r.PartialName == "zzzz-no")))
                .ReturnsAsync(new CitySearchResultDto { Cities = new List<CityDto>() });

            // ACT
            var result = await _cityAppService.SearchCitiesAsync(new CitySearchRequestDto { PartialName = "zzzz-no" });

            // ASSERT
            result.ShouldNotBeNull();
            result.Cities.ShouldBeEmpty();

            _mockCitySearchService.Verify(
                s => s.SearchCitiesAsync(It.IsAny<CitySearchRequestDto>()),
                Times.Once);
        }

        // 👇 NUEVO TEST AGREGADO 👇
        [Fact]
        public async Task GetAsync_devuelve_ciudad_por_id()
        {
            // 1. ARRANGE
            var cityId = 999;
            var expectedCity = new CityDto
            {
                Id = cityId,
                Name = "Paris",
                Country = "France"
            };

            // Configuramos el Mock: Cuando llamen a GetByIdAsync con el ID 999, devuelve la ciudad esperada
            _mockCitySearchService
                .Setup(s => s.GetByIdAsync(cityId))
                .ReturnsAsync(expectedCity);

            // 2. ACT
            var result = await _cityAppService.GetAsync(cityId);

            // 3. ASSERT
            result.ShouldNotBeNull();
            result.Id.ShouldBe(cityId);
            result.Name.ShouldBe("Paris");
            result.Country.ShouldBe("France");

            // Verificamos que el servicio interno fue llamado una vez
            _mockCitySearchService.Verify(
                s => s.GetByIdAsync(cityId),
                Times.Once);
        }
    }
}