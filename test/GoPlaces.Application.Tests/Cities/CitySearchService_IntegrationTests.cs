using System.Threading.Tasks;
using GoPlaces.Cities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace GoPlaces.Tests.Cities
{
    [Collection(GoPlacesTestConsts.CollectionDefinitionName)]
    public class CitySearchService_IntegrationTests
        : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly ICitySearchService _service;
        private readonly IConfiguration _config;

        public CitySearchService_IntegrationTests()
        {
            _service = GetRequiredService<ICitySearchService>();
            _config = GetRequiredService<IConfiguration>();
        }

        [Fact]
        public async Task SearchCitiesAsync_devuelve_resultados_reales()
        {
            // Verifica que haya una API Key configurada
            (_config["RapidApi:ApiKey"] ?? string.Empty).ShouldNotBeNullOrWhiteSpace();

            var req = new CitySearchRequestDto { PartialName = "Lon" };

            // 🔁 Reintentos con backoff exponencial para evitar fallos por rate limit o latencia
            var intento = 0;
            var maxIntentos = 6;  // Total ~15s de espera máxima
            var esperaMs = 500;
            CitySearchResultDto? res = null;

            while (intento < maxIntentos)
            {
                res = await _service.SearchCitiesAsync(req);

                if (res?.Cities?.Count > 0)
                    break;

                intento++;
                await Task.Delay(esperaMs);
                esperaMs *= 2; // 0.5s, 1s, 2s, 4s, 8s, 16s
            }

            res.ShouldNotBeNull();
            res!.Cities.ShouldNotBeEmpty(
                $"Sin resultados tras {intento + 1} intento(s). " +
                "RapidAPI pudo rate-limitear o responder vacío temporalmente."
            );
        }

        [Fact]
        public async Task SearchCitiesAsync_maneja_busqueda_sin_resultados()
        {
            var req = new CitySearchRequestDto { PartialName = "123xyz_nonexistent" };
            var res = await _service.SearchCitiesAsync(req);

            res.ShouldNotBeNull();
            res.Cities.ShouldBeEmpty();
        }
    }
}
