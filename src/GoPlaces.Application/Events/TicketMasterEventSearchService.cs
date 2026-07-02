using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GoPlaces.Events
{
    public class TicketMasterEventSearchService : ITicketMasterEventSearchService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TicketMasterEventSearchService> _logger;

        public TicketMasterEventSearchService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<TicketMasterEventSearchService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<EventSearchResultDto> SearchEventsAsync(EventSearchRequestDto request)
        {
            var city = request?.City?.Trim();
            if (string.IsNullOrWhiteSpace(city))
                return new EventSearchResultDto();

            try
            {
                var client = _httpClientFactory.CreateClient("TicketMaster");
                var apiKey = _configuration["TicketMaster:ApiKey"];

                var urlBuilder = new StringBuilder($"events.json?apikey={Uri.EscapeDataString(apiKey ?? string.Empty)}&city={Uri.EscapeDataString(city)}");

                if (request!.StartDateFrom.HasValue)
                    urlBuilder.Append($"&startDateTime={request.StartDateFrom.Value:yyyy-MM-ddTHH:mm:ssZ}");

                if (request.StartDateTo.HasValue)
                    urlBuilder.Append($"&endDateTime={request.StartDateTo.Value:yyyy-MM-ddTHH:mm:ssZ}");

                var response = await client.GetAsync(urlBuilder.ToString());
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<TicketMasterEventsApiResponse>(JsonOptions);
                var events = apiResponse?.Embedded?.Events ?? new List<TicketMasterEvent>();

                return new EventSearchResultDto
                {
                    Events = events.Select(e =>
                    {
                        var venue = e.Embedded?.Venues?.FirstOrDefault();
                        return new EventDto
                        {
                            Id = Guid.NewGuid(),
                            Name = e.Name ?? string.Empty,
                            Description = e.Info,
                            StartDate = ParseStartDate(e.Dates?.Start),
                            Venue = venue?.Name ?? string.Empty,
                            City = venue?.City?.Name ?? city,
                            Url = e.Url,
                            TicketMasterId = e.Id ?? string.Empty
                        };
                    }).ToList()
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error al conectar con la API de TicketMaster.");
                return new EventSearchResultDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en SearchEventsAsync.");
                return new EventSearchResultDto();
            }
        }

        private static DateTime ParseStartDate(TicketMasterStart? start)
        {
            if (start?.DateTime.HasValue == true)
                return start.DateTime.Value;

            if (!string.IsNullOrWhiteSpace(start?.LocalDate) && DateTime.TryParse(start.LocalDate, out var localDate))
                return localDate;

            return default;
        }

        // --- Modelos Internos (mapean el shape de la Discovery API v2 de TicketMaster) ---
        private sealed class TicketMasterEventsApiResponse
        {
            [JsonPropertyName("_embedded")]
            public TicketMasterEmbedded? Embedded { get; set; }
        }

        private sealed class TicketMasterEmbedded
        {
            public List<TicketMasterEvent> Events { get; set; } = new();
        }

        private sealed class TicketMasterEvent
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
            public string? Url { get; set; }
            public string? Info { get; set; }
            public TicketMasterDates? Dates { get; set; }

            [JsonPropertyName("_embedded")]
            public TicketMasterEventEmbedded? Embedded { get; set; }
        }

        private sealed class TicketMasterEventEmbedded
        {
            public List<TicketMasterVenue> Venues { get; set; } = new();
        }

        private sealed class TicketMasterVenue
        {
            public string? Name { get; set; }
            public TicketMasterCity? City { get; set; }
        }

        private sealed class TicketMasterCity
        {
            public string? Name { get; set; }
        }

        private sealed class TicketMasterDates
        {
            public TicketMasterStart? Start { get; set; }
        }

        private sealed class TicketMasterStart
        {
            public DateTime? DateTime { get; set; }
            public string? LocalDate { get; set; }
        }
    }
}
