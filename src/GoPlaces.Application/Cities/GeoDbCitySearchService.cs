using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace GoPlaces.Cities
{
    public class GeoDbCitySearchService : ICitySearchService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GeoDbCitySearchService> _logger;

        public GeoDbCitySearchService(IHttpClientFactory httpClientFactory, ILogger<GeoDbCitySearchService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<CitySearchResultDto> SearchCitiesAsync(CitySearchRequestDto request)
        {
            var query = request?.PartialName?.Trim();
            if (string.IsNullOrWhiteSpace(query))
                return new CitySearchResultDto { Cities = new List<CityDto>() };

            try
            {
                var client = _httpClientFactory.CreateClient("GeoDB");
                var url = $"cities?namePrefix={Uri.EscapeDataString(request.PartialName)}&limit=10";

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var geoDbApiResponse = await response.Content.ReadFromJsonAsync<GeoDbApiResponse>();
                var data = geoDbApiResponse?.Data ?? new List<GeoDbCity>();

                return new CitySearchResultDto
                {
                    Cities = data.Select(c => new CityDto
                    {
                        Id = c.Id,
                        Name = c.City ?? string.Empty,
                        Country = c.Country ?? string.Empty
                    }).ToList()
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error al conectar con la API de GeoDB.");
                return new CitySearchResultDto { Cities = new List<CityDto>() };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en SearchCitiesAsync.");
                return new CitySearchResultDto { Cities = new List<CityDto>() };
            }
        }

        // Modelos para deserializar
        private sealed class GeoDbApiResponse
        {
            public List<GeoDbCity> Data { get; set; } = new();
        }

        private sealed class GeoDbCity
        {
            public int Id { get; set; }
            public string? City { get; set; }
            public string? Country { get; set; }
        }
    }
}
