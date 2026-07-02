using System;

namespace GoPlaces.Events
{
    public class EventSearchRequestDto
    {
        public string? City { get; set; }

        /// <summary>Fecha mínima de inicio del evento. Opcional.</summary>
        public DateTime? StartDateFrom { get; set; }

        /// <summary>Fecha máxima de inicio del evento. Opcional.</summary>
        public DateTime? StartDateTo { get; set; }

        /// <summary>Si se especifica, los eventos encontrados quedan asociados a este destino.</summary>
        public Guid? DestinationId { get; set; }
    }
}
