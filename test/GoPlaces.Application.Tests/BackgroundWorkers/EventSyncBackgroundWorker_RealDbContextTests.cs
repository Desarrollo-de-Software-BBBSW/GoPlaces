using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GoPlaces.BackgroundWorkers;
using GoPlaces.Follows;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Guids;
using Volo.Abp.Security.Claims;
using Xunit;

namespace GoPlaces.Tests.BackgroundWorkers
{
    // A diferencia de EventSyncBackgroundWorker_Tests (todo mockeado, incluido IUnitOfWorkManager),
    // estos tests resuelven el worker real desde el contenedor de DI de GoPlacesApplicationTestModule,
    // que usa un DbContext real (Sqlite in-memory) — el mismo mecanismo que ya usan FollowAppService_Tests
    // y NotificationAppService_Tests. Reproducen el escenario del bug real encontrado corriendo
    // `dotnet run`: el worker no es un ApplicationService, así que sin un Unit of Work explícito el
    // DbContext efímero de una llamada a repositorio se cerraba antes de que otra query se
    // materializara, tirando ObjectDisposedException. Un test con mocks nunca lo iba a detectar
    // porque un IQueryable mockeado (List<T>.AsQueryable()) es LINQ-to-Objects puro: no tiene ningún
    // concepto de "contexto disponible/disponible" para romperse.
    [Collection(GoPlacesTestConsts.CollectionDefinitionName)]
    public class EventSyncBackgroundWorker_RealDbContextTests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly EventSyncBackgroundWorker _worker;
        private readonly IFollowAppService _followAppService;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

        public EventSyncBackgroundWorker_RealDbContextTests()
        {
            _worker = GetRequiredService<EventSyncBackgroundWorker>();
            _followAppService = GetRequiredService<IFollowAppService>();
            _guidGenerator = GetRequiredService<IGuidGenerator>();
            _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
        }

        private IDisposable ChangeUserContext(Guid userId, string userName)
        {
            var claims = new[]
            {
                new Claim(AbpClaimTypes.UserId, userId.ToString()),
                new Claim(AbpClaimTypes.UserName, userName)
            };
            return _currentPrincipalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));
        }

        [Fact]
        public async Task SyncFollowedDestinationsAsync_no_explota_con_DbContext_real_sin_UnitOfWork_ambiente()
        {
            var userId = _guidGenerator.Create();
            // No creamos ningún Destination con este id a propósito: alcanza para ejercitar tanto
            // la query de FollowLists como la búsqueda posterior del destino (las dos llamadas a
            // repositorio que están en el mismo método donde se reprodujo el ObjectDisposedException).
            var destinationId = _guidGenerator.Create();

            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(userId, "traveler_test"))
                {
                    await _followAppService.SaveDestinationAsync(new SaveOrRemoveInputDto { DestinationId = destinationId });
                }
            });

            using var scope = ServiceProvider.CreateScope();

            // Antes del fix, esto tiraba ObjectDisposedException: "Cannot access a disposed
            // context instance" al materializar la query de FollowLists dentro de
            // GetFollowedDestinationIdsAsync.
            await Should.NotThrowAsync(() => _worker.SyncFollowedDestinationsAsync(scope.ServiceProvider));
        }
    }
}
