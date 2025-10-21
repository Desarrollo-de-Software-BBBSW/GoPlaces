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
            _mockCitySearchService = new Mock<ICitySearchService>();
            _cityAppService = new CityAppService(_mockCitySearchService.Object);
        }

        [Fact]
        public async Task SearchCitiesAsync_devuelve_resultados_del_servicio()
        {
            _mockCitySearchService
                .Setup(s => s.SearchCitiesAsync(It.Is<CitySearchRequestDto>(r => r.PartialName == "Lon")))
                .ReturnsAsync(new CitySearchResultDto
                {
                    Cities = new List<CityDto>
                    {
                        new() { Id = 1, Name = "London", Country = "United Kingdom" }
                    }
                });

            var result = await _cityAppService.SearchCitiesAsync(new CitySearchRequestDto { PartialName = "Lon" });

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
            _mockCitySearchService
                .Setup(s => s.SearchCitiesAsync(It.Is<CitySearchRequestDto>(r => r.PartialName == "zzzz-no")))
                .ReturnsAsync(new CitySearchResultDto { Cities = new List<CityDto>() });

            var result = await _cityAppService.SearchCitiesAsync(new CitySearchRequestDto { PartialName = "zzzz-no" });

            result.ShouldNotBeNull();
            result.Cities.ShouldBeEmpty();

            _mockCitySearchService.Verify(
                s => s.SearchCitiesAsync(It.IsAny<CitySearchRequestDto>()),
                Times.Once);
        }
    }
}
