using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Volo.Abp; // Necesario para UserFriendlyException (opcional, si quieres usarlo)

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

        // 👇 NUEVA IMPLEMENTACIÓN DEL MÉTODO QUE FALTABA 👇
        public async Task<CityDto> GetByIdAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("GeoDB");
                // Endpoint para obtener una sola ciudad por ID
                var url = $"cities/{id}";

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    // Si no existe (404) o hay error, lanzamos excepción para que la capa superior se entere
                    throw new UserFriendlyException($"No se pudo encontrar la ciudad con ID {id}. API Status: {response.StatusCode}");
                }

                // OJO: La respuesta de GeoDB para un item singular es diferente (Data es un objeto, no una lista)
                var result = await response.Content.ReadFromJsonAsync<GeoDbSingleApiResponse>();

                if (result?.Data == null)
                {
                    throw new UserFriendlyException("La API no devolvió datos para esta ciudad.");
                }

                return new CityDto
                {
                    Id = result.Data.Id,
                    Name = result.Data.City ?? string.Empty,
                    Country = result.Data.Country ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error obteniendo detalle de ciudad {id}");
                throw; // Relanzamos el error para que lo maneje el Controller/AppService
            }
        }

        // --- Modelos para deserializar ---

        // Modelo para respuesta de LISTAS (Search)
        private sealed class GeoDbApiResponse
        {
            public List<GeoDbCity> Data { get; set; } = new();
        }

        // 👇 NUEVO MODELO para respuesta INDIVIDUAL (GetById)
        private sealed class GeoDbSingleApiResponse
        {
            public GeoDbCity Data { get; set; } // Aquí Data es un solo objeto, no una lista
        }

        // El objeto ciudad es el mismo para ambos casos
        private sealed class GeoDbCity
        {
            public int Id { get; set; }
            public string? City { get; set; }
            public string? Country { get; set; }
        }
    }
}