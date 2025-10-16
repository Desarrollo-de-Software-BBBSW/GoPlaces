using Microsoft.Extensions.Configuration; // Para leer la API Key
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json; // Necesario para ReadFromJsonAsync
using System.Text;
using System.Threading.Tasks;

namespace GoPlaces.Cities
{
    public class GeoDbCitySearchService : ICitySearchService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeoDbCitySearchService> _logger;

        // Inyectamos IHttpClientFactory para crear clientes HTTP
        // Inyectamos IConfiguration para leer secretos (como la API Key)
        public GeoDbCitySearchService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GeoDbCitySearchService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<CitySearchResultDto> SearchCitiesAsync(CitySearchRequestDto request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("GeoDB");
                var response = await client.GetAsync($"cities?limit=5&minPopulation=100000&namePrefix={request.PartialName}");

                response.EnsureSuccessStatusCode();

                // Procedes a leer y mapear los datos.
                var geoDbApiResponse = await response.Content.ReadFromJsonAsync<GeoDbApiResponse>();
                var result = new CitySearchResultDto
                {
                    Cities = geoDbApiResponse.Data.Select(c => new CityDto
                    {
                        Id = c.Id,
                        Name = c.City,      // <-- Asigna la propiedad "City" de la API a "Name"
                        Country = c.Country // <-- Asigna "Country" a "Country"
                    }).ToList()
                };
                return result;


            }
            catch (HttpRequestException ex)
            {
                // Este bloque CATCH se ejecuta si:
                // 1. Ocurre un error de red/conexión (el servidor no responde, timeout, etc.).
                // 2. Usaste `response.EnsureSuccessStatusCode()` y la respuesta no fue exitosa.

                _logger.LogError(ex, "Error al conectar con la API de GeoDB."); // <-- Usa la variable inyectada

                // Devuelves una respuesta vacía y controlada al usuario final.
                return new CitySearchResultDto { Cities = new List<CityDto>() };
            }
        }
    }

    // Clases de ejemplo para deserializar la respuesta de GeoDB API
    // Debes ajustarlas a la estructura real del JSON que devuelve la API
    public class GeoDbApiResponse { public List<GeoDbCity> Data { get; set; } }
    public class GeoDbCity { public int Id { get; set; } public string City { get; set; } public string Country { get; set; } }
}
