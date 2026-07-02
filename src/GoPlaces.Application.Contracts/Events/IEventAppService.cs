using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GoPlaces.Events
{
    public interface IEventAppService : IApplicationService
    {
        /// <summary>Busca eventos de TicketMaster para una ciudad, aplicando filtros opcionales de fecha.</summary>
        Task<EventSearchResultDto> SearchEventsByCityAsync(EventSearchRequestDto request);

        /// <summary>Devuelve los eventos ya sincronizados que están asociados a un destino.</summary>
        Task<List<EventDto>> GetEventsByDestinationAsync(Guid destinationId);
    }
}
