using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GoPlaces.BackgroundWorkers;
using GoPlaces.Events;
using GoPlaces.Follows;
using GoPlaces.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Xunit;
using DestinationEntity = GoPlaces.Destinations.Destination;
using DestinationCoordinates = GoPlaces.Destinations.Coordinates;

namespace GoPlaces.Tests.BackgroundWorkers
{
    // IMPORTANTE: estos tests mockean IRepository/IUnitOfWorkManager, así que nunca hay un
    // DbContext real de por medio. Esto los hace rápidos y buenos para probar la lógica de
    // negocio (cuándo notificar, cuándo loguear y seguir), pero son ciegos a bugs de lifetime de
    // EF Core — por ejemplo, el ObjectDisposedException real que se reprodujo corriendo el
    // worker con `dotnet run` (un DbContext efímero se cerraba antes de que se materializara una
    // query hecha en otra llamada). Ese tipo de bug solo lo detecta un test con un DbContext real
    // (ver EventSyncBackgroundWorker_RealDbContextTests, que usa el mismo Sqlite in-memory que el
    // resto de los tests de Application.Tests).
    public class EventSyncBackgroundWorker_Tests
    {
        private readonly Mock<IRepository<FollowList, Guid>> _mockFollowListRepository;
        private readonly Mock<IRepository<DestinationEntity, Guid>> _mockDestinationRepository;
        private readonly Mock<IEventAppService> _mockEventAppService;
        private readonly Mock<DestinationNotificationDomainService> _mockDestinationNotificationDomainService;
        private readonly Mock<ILogger<EventSyncBackgroundWorker>> _mockLogger;
        private readonly Mock<IUnitOfWorkManager> _mockUnitOfWorkManager;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly IServiceProvider _serviceProvider;
        private readonly EventSyncBackgroundWorker _worker;

        public EventSyncBackgroundWorker_Tests()
        {
            _mockFollowListRepository = new Mock<IRepository<FollowList, Guid>>();
            _mockDestinationRepository = new Mock<IRepository<DestinationEntity, Guid>>();
            _mockEventAppService = new Mock<IEventAppService>();
            // Domain Service concreto: se le pasan repos dummy al ctor, y se mockea el método
            // virtual NotifyDestinationChangeAsync para no depender de infraestructura real.
            _mockDestinationNotificationDomainService = new Mock<DestinationNotificationDomainService>(
                Mock.Of<IRepository<Notification, Guid>>(),
                Mock.Of<IRepository<FollowList, Guid>>());
            _mockLogger = new Mock<ILogger<EventSyncBackgroundWorker>>();

            // El worker abre un Unit of Work explícito por cada destino (y otro para la query de
            // seguidos); acá el manager solo devuelve un IUnitOfWork mockeado que no hace nada.
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
            _mockUnitOfWorkManager
                .Setup(m => m.Begin(It.IsAny<AbpUnitOfWorkOptions>(), It.IsAny<bool>()))
                .Returns(_mockUnitOfWork.Object);

            // El logger ya no se resuelve desde acá: EventSyncBackgroundWorker lo recibe por
            // constructor (ver más abajo), no lo pide al serviceProvider de cada ciclo.
            var services = new ServiceCollection();
            services.AddSingleton(_mockFollowListRepository.Object);
            services.AddSingleton(_mockDestinationRepository.Object);
            services.AddSingleton(_mockEventAppService.Object);
            services.AddSingleton(_mockDestinationNotificationDomainService.Object);
            services.AddSingleton(_mockUnitOfWorkManager.Object);
            _serviceProvider = services.BuildServiceProvider();

            var configuration = new ConfigurationBuilder().Build();
            _worker = new EventSyncBackgroundWorker(
                new AbpAsyncTimer(),
                _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                configuration,
                _mockLogger.Object);
        }

        private void SetupFollowedDestinations(params Guid[] destinationIds)
        {
            var followList = new FollowList(Guid.NewGuid(), Guid.NewGuid(), "Favoritos");
            foreach (var destinationId in destinationIds)
            {
                followList.AddDestination(destinationId);
            }

            _mockFollowListRepository
                .Setup(r => r.WithDetailsAsync(It.IsAny<Expression<Func<FollowList, object>>[]>()))
                .ReturnsAsync(new List<FollowList> { followList }.AsQueryable());
        }

        private void SetupDestination(Guid destinationId, string name = "Buenos Aires")
        {
            var destination = new DestinationEntity(destinationId, name, "Argentina", 3_000_000, new DestinationCoordinates(-34.6, -58.4));
            _mockDestinationRepository
                .Setup(r => r.FindAsync(destinationId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(destination);
        }

        [Fact]
        public async Task SyncFollowedDestinationsAsync_notifica_cuando_hay_eventos_nuevos()
        {
            // ARRANGE
            var destinationId = Guid.NewGuid();
            SetupFollowedDestinations(destinationId);
            SetupDestination(destinationId);

            _mockEventAppService
                .Setup(s => s.GetEventsByDestinationAsync(destinationId))
                .ReturnsAsync(new List<EventDto>()); // sin eventos previos

            _mockEventAppService
                .Setup(s => s.SearchEventsByCityAsync(It.IsAny<EventSearchRequestDto>()))
                .ReturnsAsync(new EventSearchResultDto
                {
                    Events = new List<EventDto> { new() { TicketMasterId = "TM-1", Name = "Concierto" } }
                });

            // ACT
            await _worker.SyncFollowedDestinationsAsync(_serviceProvider);

            // ASSERT
            _mockDestinationNotificationDomainService.Verify(
                s => s.NotifyDestinationChangeAsync(destinationId, It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task SyncFollowedDestinationsAsync_no_notifica_cuando_no_hay_eventos_nuevos()
        {
            // ARRANGE
            var destinationId = Guid.NewGuid();
            SetupFollowedDestinations(destinationId);
            SetupDestination(destinationId);

            _mockEventAppService
                .Setup(s => s.GetEventsByDestinationAsync(destinationId))
                .ReturnsAsync(new List<EventDto> { new() { TicketMasterId = "TM-1", Name = "Concierto" } });

            _mockEventAppService
                .Setup(s => s.SearchEventsByCityAsync(It.IsAny<EventSearchRequestDto>()))
                .ReturnsAsync(new EventSearchResultDto
                {
                    Events = new List<EventDto> { new() { TicketMasterId = "TM-1", Name = "Concierto" } }
                });

            // ACT
            await _worker.SyncFollowedDestinationsAsync(_serviceProvider);

            // ASSERT
            _mockDestinationNotificationDomainService.Verify(
                s => s.NotifyDestinationChangeAsync(It.IsAny<Guid>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task SyncFollowedDestinationsAsync_loggea_error_y_continua_con_los_demas_destinos()
        {
            // ARRANGE: dos destinos seguidos, el primero falla al sincronizar
            var destinationIdFalla = Guid.NewGuid();
            var destinationIdOk = Guid.NewGuid();
            SetupFollowedDestinations(destinationIdFalla, destinationIdOk);
            SetupDestination(destinationIdFalla, "Ciudad Rota");
            SetupDestination(destinationIdOk, "Ciudad Ok");

            _mockEventAppService
                .Setup(s => s.GetEventsByDestinationAsync(destinationIdFalla))
                .ThrowsAsync(new InvalidOperationException("Error simulado de base de datos"));

            _mockEventAppService
                .Setup(s => s.GetEventsByDestinationAsync(destinationIdOk))
                .ReturnsAsync(new List<EventDto>());

            _mockEventAppService
                .Setup(s => s.SearchEventsByCityAsync(It.Is<EventSearchRequestDto>(r => r.DestinationId == destinationIdOk)))
                .ReturnsAsync(new EventSearchResultDto
                {
                    Events = new List<EventDto> { new() { TicketMasterId = "TM-2", Name = "Festival" } }
                });

            // ACT
            await _worker.SyncFollowedDestinationsAsync(_serviceProvider);

            // ASSERT: se loggeó el error del destino roto...
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // ...y el worker siguió con el destino sano, notificando solo por ese
            _mockDestinationNotificationDomainService.Verify(
                s => s.NotifyDestinationChangeAsync(destinationIdOk, It.IsAny<string>()),
                Times.Once);
            _mockDestinationNotificationDomainService.Verify(
                s => s.NotifyDestinationChangeAsync(destinationIdFalla, It.IsAny<string>()),
                Times.Never);

            // No debe haberse llamado a buscar eventos del destino roto (se cayó antes, en GetEventsByDestinationAsync)
            _mockEventAppService.Verify(
                s => s.SearchEventsByCityAsync(It.Is<EventSearchRequestDto>(r => r.DestinationId == destinationIdFalla)),
                Times.Never);
        }

        [Fact]
        public async Task SyncFollowedDestinationsAsync_no_hace_nada_si_no_hay_destinos_seguidos()
        {
            // ARRANGE: ninguna FollowList tiene destinos
            _mockFollowListRepository
                .Setup(r => r.WithDetailsAsync(It.IsAny<Expression<Func<FollowList, object>>[]>()))
                .ReturnsAsync(new List<FollowList>().AsQueryable());

            // ACT & ASSERT: no debe tirar ninguna excepción
            await Should.NotThrowAsync(() => _worker.SyncFollowedDestinationsAsync(_serviceProvider));

            // ASSERT: no se llamó a sincronizar ni a notificar nada
            _mockEventAppService.Verify(
                s => s.SearchEventsByCityAsync(It.IsAny<EventSearchRequestDto>()),
                Times.Never);
            _mockDestinationNotificationDomainService.Verify(
                s => s.NotifyDestinationChangeAsync(It.IsAny<Guid>(), It.IsAny<string>()),
                Times.Never);
        }
    }
}
