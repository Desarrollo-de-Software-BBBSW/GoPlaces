using System;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace GoPlaces.Events
{
    /// <summary>
    /// Servicio de dominio que encapsula las reglas de negocio para los filtros de búsqueda de eventos.
    /// </summary>
    public class EventSearchDomainService : DomainService
    {
        /// <summary>
        /// Valida que los filtros de búsqueda de eventos sean coherentes con las reglas de negocio.
        /// </summary>
        public void ValidateFilters(string? city, DateTime? startDateFrom, DateTime? startDateTo)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                throw new UserFriendlyException(
                    "Debe especificar una ciudad para buscar eventos.",
                    "GoPlaces:CityIsRequiredForEventSearch"
                );
            }

            if (startDateFrom.HasValue && startDateTo.HasValue && startDateFrom.Value > startDateTo.Value)
            {
                throw new UserFriendlyException(
                    "La fecha de inicio no puede ser posterior a la fecha de fin.",
                    "GoPlaces:InvalidEventDateRange"
                );
            }
        }
    }
}
