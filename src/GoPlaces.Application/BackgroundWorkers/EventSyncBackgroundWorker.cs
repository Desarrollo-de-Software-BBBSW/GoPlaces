using GoPlaces.Destinations;
using GoPlaces.Events;
using GoPlaces.Follows;
using GoPlaces.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace GoPlaces.BackgroundWorkers
{
    // Sincroniza eventos de TicketMaster para los destinos que tienen al menos un usuario
    // siguiéndolos, y notifica a esos usuarios cuando aparecen eventos nuevos.
    public class EventSyncBackgroundWorker : AsyncPeriodicBackgroundWorkerBase
    {
        private const int DefaultIntervalHours = 12;
        private static readonly TimeSpan SyncWindow = TimeSpan.FromDays(180); // ~6 meses

        // Inyectado por constructor (no resuelto por request/scope, a diferencia de los repos):
        // ILogger<T> no está atado al ciclo de vida de ningún DbContext ni Unit of Work, así que
        // es seguro guardarlo en la instancia larga del worker, igual que hace el sample oficial
        // de ABP con la propiedad Logger heredada.
        private readonly ILogger<EventSyncBackgroundWorker> _logger;

        public EventSyncBackgroundWorker(
            AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration,
            ILogger<EventSyncBackgroundWorker> logger) : base(timer, serviceScopeFactory)
        {
            _logger = logger;

            // IntervalMinutes tiene prioridad sobre IntervalHours: permite probar el worker
            // en desarrollo sin esperar horas reales. En producción alcanza con IntervalHours.
            var intervalMinutes = configuration.GetValue<int?>("EventSyncWorker:IntervalMinutes");
            var period = intervalMinutes.HasValue
                ? TimeSpan.FromMinutes(intervalMinutes.Value)
                : TimeSpan.FromHours(configuration.GetValue<int?>("EventSyncWorker:IntervalHours") ?? DefaultIntervalHours);

            Timer.Period = (int)period.TotalMilliseconds;

            // Etapa 1: confirma en el log que el worker arrancó al iniciar el Host (se registra
            // una sola vez, cuando AddBackgroundWorkerAsync resuelve/construye la instancia).
            _logger.LogInformation("EventSyncBackgroundWorker inicializado. Sincronizará cada {Period}.", period);
        }

        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            await SyncFollowedDestinationsAsync(workerContext.ServiceProvider, workerContext.CancellationToken);
        }

        // Separado de DoWorkAsync para poder invocarlo directamente desde los tests unitarios
        // (pasando un IServiceProvider armado a mano con los mocks) sin depender del timer real.
        public async Task SyncFollowedDestinationsAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            var unitOfWorkManager = serviceProvider.GetRequiredService<IUnitOfWorkManager>();
            var followListRepository = serviceProvider.GetRequiredService<IRepository<FollowList, Guid>>();
            var destinationRepository = serviceProvider.GetRequiredService<IRepository<Destination, Guid>>();
            var eventAppService = serviceProvider.GetRequiredService<IEventAppService>();
            var destinationNotificationDomainService = serviceProvider.GetRequiredService<DestinationNotificationDomainService>();

            // Etapa 2: arranca un nuevo ciclo de sincronización.
            _logger.LogInformation("EventSyncBackgroundWorker: iniciando ciclo de sincronización de eventos.");

            var followedDestinationIds = await GetFollowedDestinationIdsAsync(unitOfWorkManager, followListRepository);
            if (followedDestinationIds.Count == 0)
            {
                // Etapa 5a: no hay destinos seguidos.
                _logger.LogInformation("EventSyncBackgroundWorker: no hay destinos seguidos, no hay nada que sincronizar.");
                return;
            }

            _logger.LogInformation(
                "EventSyncBackgroundWorker: {Count} destino(s) seguido(s) encontrados, sincronizando eventos.",
                followedDestinationIds.Count);

            var startDateFrom = DateTime.UtcNow.Date;
            var startDateTo = startDateFrom.Add(SyncWindow);

            foreach (var destinationId in followedDestinationIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Este worker no es un ApplicationService, así que ABP no lo envuelve en un
                    // Unit of Work automáticamente. Sin este Begin() explícito, cada llamada a un
                    // repositorio abre y cierra su propio DbContext efímero: si una query se
                    // materializa (ToList) después de que ese contexto ya se cerró, EF Core tira
                    // ObjectDisposedException (bug real detectado corriendo el worker de verdad,
                    // no lo detectaban los tests porque ahí todo está mockeado).
                    using var uow = unitOfWorkManager.Begin(new AbpUnitOfWorkOptions(isTransactional: true), requiresNew: true);

                    var destination = await destinationRepository.FindAsync(destinationId, cancellationToken: cancellationToken);
                    if (destination == null)
                    {
                        _logger.LogWarning(
                            "EventSyncBackgroundWorker: el destino {DestinationId} está seguido pero no existe en la tabla de destinos; se omite.",
                            destinationId);
                    }
                    else
                    {
                        // Etapa 3: procesando este destino seguido en particular.
                        _logger.LogInformation(
                            "EventSyncBackgroundWorker: sincronizando eventos de TicketMaster para el destino {DestinationId} ({DestinationName}).",
                            destinationId, destination.Name);

                        var eventosPrevios = await eventAppService.GetEventsByDestinationAsync(destinationId);
                        var ticketMasterIdsPrevios = eventosPrevios.Select(e => e.TicketMasterId).ToHashSet();

                        var result = await eventAppService.SearchEventsByCityAsync(new EventSearchRequestDto
                        {
                            City = destination.Name,
                            DestinationId = destinationId,
                            StartDateFrom = startDateFrom,
                            StartDateTo = startDateTo
                        });

                        var eventosNuevos = result.Events.Where(e => !ticketMasterIdsPrevios.Contains(e.TicketMasterId)).ToList();
                        if (eventosNuevos.Count > 0)
                        {
                            // Etapa 4: eventos nuevos detectados, se dispara la notificación.
                            _logger.LogInformation(
                                "EventSyncBackgroundWorker: {NuevosCount} evento(s) nuevo(s) encontrados para el destino {DestinationId}, notificando a los seguidores.",
                                eventosNuevos.Count, destinationId);

                            await destinationNotificationDomainService.NotifyDestinationChangeAsync(
                                destinationId,
                                "Encontramos nuevos eventos para este destino.");
                        }
                        else
                        {
                            // Etapa 5b: no hay eventos nuevos para este destino puntual.
                            _logger.LogInformation(
                                "EventSyncBackgroundWorker: sin eventos nuevos para el destino {DestinationId}.",
                                destinationId);
                        }
                    }

                    // Importante: tiene que llamarse en todos los caminos que no sean error
                    // (destino inexistente, sin eventos nuevos, o con notificación disparada).
                    // Si se corta antes con un "continue", el UOW transaccional hace rollback al
                    // disponerse y se pierden los inserts que EventAppService ya hizo con autoSave.
                    await uow.CompleteAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al sincronizar eventos de TicketMaster para el destino {DestinationId}.", destinationId);
                }
            }
        }

        private static async Task<List<Guid>> GetFollowedDestinationIdsAsync(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<FollowList, Guid> followListRepository)
        {
            using var uow = unitOfWorkManager.Begin(new AbpUnitOfWorkOptions(isTransactional: false), requiresNew: true);

            var queryable = await followListRepository.WithDetailsAsync(x => x.Items);
            var followLists = queryable.ToList();

            await uow.CompleteAsync();

            return followLists
                .SelectMany(list => list.Items.Select(item => item.DestinationId))
                .Distinct()
                .ToList();
        }
    }
}
