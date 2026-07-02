using System.Threading.Tasks;

namespace GoPlaces.Events
{
    /// <summary>Abstracción de la llamada cruda a la API de TicketMaster (Discovery API).</summary>
    public interface ITicketMasterEventSearchService
    {
        Task<EventSearchResultDto> SearchEventsAsync(EventSearchRequestDto request);
    }
}
