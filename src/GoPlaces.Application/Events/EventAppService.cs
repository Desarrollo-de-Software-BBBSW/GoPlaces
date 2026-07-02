using GoPlaces.ExternalApiMetrics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace GoPlaces.Events
{
    public class EventAppService : ApplicationService, IEventAppService
    {
        private readonly ITicketMasterEventSearchService _ticketMasterEventSearchService;
        private readonly IRepository<Event, Guid> _eventRepository;
        private readonly IRepository<ExternalApiCall, Guid> _externalApiCallRepository;
        private readonly EventSearchDomainService _eventSearchDomainService;

        public EventAppService(
            ITicketMasterEventSearchService ticketMasterEventSearchService,
            IRepository<Event, Guid> eventRepository,
            IRepository<ExternalApiCall, Guid> externalApiCallRepository,
            EventSearchDomainService eventSearchDomainService)
        {
            _ticketMasterEventSearchService = ticketMasterEventSearchService;
            _eventRepository = eventRepository;
            _externalApiCallRepository = externalApiCallRepository;
            _eventSearchDomainService = eventSearchDomainService;
        }

        public async Task<EventSearchResultDto> SearchEventsByCityAsync(EventSearchRequestDto request)
        {
            _eventSearchDomainService.ValidateFilters(request?.City, request?.StartDateFrom, request?.StartDateTo);

            var endpointBuilder = new StringBuilder($"events.json?city={request?.City}");
            if (request?.StartDateFrom.HasValue == true)
                endpointBuilder.Append($"&startDateTime={request.StartDateFrom:yyyy-MM-dd}");
            if (request?.StartDateTo.HasValue == true)
                endpointBuilder.Append($"&endDateTime={request.StartDateTo:yyyy-MM-dd}");

            var endpoint = endpointBuilder.ToString();
            var stopwatch = Stopwatch.StartNew();
            var isSuccess = false;

            try
            {
                var result = await _ticketMasterEventSearchService.SearchEventsAsync(request!);
                isSuccess = true;

                await UpsertEventsAsync(result.Events, request?.DestinationId);

                return result;
            }
            catch (Exception ex)
            {
                // La API externa no debe tirar abajo el flujo: se loggea y se devuelve un resultado vacío.
                Logger.LogError(ex, "Error al buscar eventos con TicketMaster.");
                return new EventSearchResultDto();
            }
            finally
            {
                stopwatch.Stop();
                await _externalApiCallRepository.InsertAsync(
                    new ExternalApiCall(Guid.NewGuid(), "TicketMaster", endpoint, (int)stopwatch.ElapsedMilliseconds, isSuccess),
                    autoSave: true
                );
            }
        }

        public async Task<List<EventDto>> GetEventsByDestinationAsync(Guid destinationId)
        {
            var queryable = await _eventRepository.GetQueryableAsync();
            var events = queryable
                .Where(x => x.DestinationId == destinationId)
                .OrderBy(x => x.StartDate);

            var list = await AsyncExecuter.ToListAsync(events);
            return list.Select(MapToDto).ToList();
        }

        private async Task UpsertEventsAsync(List<EventDto> events, Guid? destinationId)
        {
            foreach (var eventDto in events)
            {
                var existing = await _eventRepository.FindAsync(x => x.TicketMasterId == eventDto.TicketMasterId);

                if (existing != null)
                {
                    // El mismo evento de TicketMaster puede haberse sincronizado antes sin destino
                    // asociado (o desde otra búsqueda). Si ahora llega un destinationId, hay que
                    // vincularlo; si no, el evento queda huérfano para siempre.
                    if (destinationId.HasValue && existing.DestinationId != destinationId)
                    {
                        existing.SetDestinationId(destinationId);
                        await _eventRepository.UpdateAsync(existing, autoSave: true);
                    }

                    continue;
                }

                await _eventRepository.InsertAsync(
                    new Event(
                        Guid.NewGuid(),
                        eventDto.Name,
                        eventDto.StartDate,
                        eventDto.Venue,
                        eventDto.City,
                        eventDto.TicketMasterId,
                        eventDto.Description,
                        eventDto.Url,
                        destinationId
                    ),
                    autoSave: true
                );
            }
        }

        private static EventDto MapToDto(Event entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            StartDate = entity.StartDate,
            Venue = entity.Venue,
            City = entity.City,
            Url = entity.Url,
            TicketMasterId = entity.TicketMasterId,
            DestinationId = entity.DestinationId
        };
    }
}
