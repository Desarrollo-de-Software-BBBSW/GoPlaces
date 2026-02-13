using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Volo.Abp;

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

        // Helper para convertir int a Guid de forma consistente
        // Ej: ID 5 -> 00000000-0000-0000-0000-000000000005
        private Guid IntToGuid(int value)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
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
                        // CONVERSIÓN CRÍTICA: Int -> Guid
                        Id = IntToGuid(c.Id),
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

        // CAMBIO: Recibe Guid en lugar de int
        public async Task<CityDto> GetByIdAsync(Guid id)
        {
            // Convertimos el Guid de vuelta a int para llamar a la API
            // (Tomamos los primeros 4 bytes que es donde guardamos el int)
            int geoDbId = BitConverter.ToInt32(id.ToByteArray(), 0);

            try
            {
                var client = _httpClientFactory.CreateClient("GeoDB");
                var url = $"cities/{geoDbId}"; // Usamos el ID numérico original

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new UserFriendlyException($"No se pudo encontrar la ciudad. API Status: {response.StatusCode}");
                }

                var result = await response.Content.ReadFromJsonAsync<GeoDbSingleApiResponse>();

                if (result?.Data == null)
                {
                    throw new UserFriendlyException("La API no devolvió datos para esta ciudad.");
                }

                return new CityDto
                {
                    Id = IntToGuid(result.Data.Id), // Volvemos a convertir a Guid
                    Name = result.Data.City ?? string.Empty,
                    Country = result.Data.Country ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error obteniendo detalle de ciudad {id}");
                throw;
            }
        }

        // --- Modelos Internos (Se mantienen con int porque así viene de la API externa) ---
        private sealed class GeoDbApiResponse
        {
            public List<GeoDbCity> Data { get; set; } = new();
        }

        private sealed class GeoDbSingleApiResponse
        {
            public GeoDbCity Data { get; set; }
        }

        private sealed class GeoDbCity
        {
            public int Id { get; set; } // La API externa sigue enviando int
            public string? City { get; set; }
            public string? Country { get; set; }
        }
    }
}