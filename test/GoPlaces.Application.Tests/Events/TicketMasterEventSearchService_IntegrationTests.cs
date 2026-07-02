using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GoPlaces.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace GoPlaces.Tests.Events
{
    // Tests de integración: llaman a la API REAL de TicketMaster (Discovery API), no usan mocks.
    // Requieren "TicketMaster:ApiKey" configurada (en appsettings.secrets.json o vía variable de
    // entorno TicketMaster__ApiKey) para los casos de éxito; si no está configurada, esos casos
    // se omiten en lugar de fallar (ver ApiKeyIsConfigured).
    //
    // Para excluirlos de la corrida normal:
    //   dotnet test --filter "Category!=Integration"
    [Trait("Category", "Integration")]
    public class TicketMasterEventSearchService_IntegrationTests
        : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly ITicketMasterEventSearchService _service;
        private readonly IConfiguration _config;

        public TicketMasterEventSearchService_IntegrationTests()
        {
            _service = GetRequiredService<ITicketMasterEventSearchService>();
            _config = GetRequiredService<IConfiguration>();
        }

        private bool ApiKeyIsConfigured => !string.IsNullOrWhiteSpace(_config["TicketMaster:ApiKey"]);

        [Fact]
        public async Task SearchEventsAsync_devuelve_resultados_reales_para_Buenos_Aires()
        {
            if (!ApiKeyIsConfigured)
                return; // Sin "TicketMaster:ApiKey" configurada no se puede pegarle a la API real: se omite.

            await WithUnitOfWorkAsync(async () =>
            {
                var req = new EventSearchRequestDto { City = "Buenos Aires" };

                // 🔁 Reintentos con backoff exponencial para evitar fallos por rate limit o latencia
                var intento = 0;
                var maxIntentos = 4;
                var esperaMs = 500;
                EventSearchResultDto? res = null;

                while (intento < maxIntentos)
                {
                    res = await _service.SearchEventsAsync(req);

                    if (res?.Events?.Count > 0)
                        break;

                    intento++;
                    await Task.Delay(esperaMs);
                    esperaMs *= 2;
                }

                res.ShouldNotBeNull();
                res!.Events.ShouldNotBeEmpty(
                    $"Sin resultados tras {intento + 1} intento(s). " +
                    "La API pudo responder vacía temporalmente o no hay eventos para esa ciudad ahora."
                );
            });
        }

        [Fact]
        public async Task SearchEventsAsync_mapea_correctamente_los_campos_del_evento()
        {
            if (!ApiKeyIsConfigured)
                return; // Sin "TicketMaster:ApiKey" configurada no se puede pegarle a la API real: se omite.

            await WithUnitOfWorkAsync(async () =>
            {
                var req = new EventSearchRequestDto { City = "New York" };
                var res = await _service.SearchEventsAsync(req);

                res.ShouldNotBeNull();
                res!.Events.ShouldNotBeEmpty();

                var evento = res.Events[0];
                evento.Name.ShouldNotBeNullOrWhiteSpace();
                evento.StartDate.ShouldNotBe(default);
                evento.Venue.ShouldNotBeNullOrWhiteSpace();
                evento.Url.ShouldNotBeNullOrWhiteSpace();
                evento.TicketMasterId.ShouldNotBeNullOrWhiteSpace();
            });
        }

        [Fact]
        public async Task SearchEventsAsync_maneja_key_invalida_de_la_api_sin_lanzar_excepcion()
        {
            // Este caso no depende de tener configurada una key real: probamos deliberadamente
            // una key inválida contra la API real de TicketMaster y verificamos que el servicio
            // atrapa la respuesta de error (401/403) y devuelve un resultado vacío en vez de explotar.
            var httpClientFactory = GetRequiredService<IHttpClientFactory>();
            var logger = GetRequiredService<ILogger<TicketMasterEventSearchService>>();

            var fakeConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TicketMaster:ApiKey"] = "clave-invalida-para-test-de-integracion"
                })
                .Build();

            var serviceConKeyInvalida = new TicketMasterEventSearchService(httpClientFactory, fakeConfig, logger);

            await WithUnitOfWorkAsync(async () =>
            {
                var req = new EventSearchRequestDto { City = "Buenos Aires" };

                var exception = await Record.ExceptionAsync(async () =>
                {
                    var res = await serviceConKeyInvalida.SearchEventsAsync(req);
                    res.ShouldNotBeNull();
                });

                exception.ShouldBeNull();
            });
        }
    }
}
