using System;
using Volo.Abp.Application.Dtos;

namespace GoPlaces.Events
{
    public class EventDto : EntityDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public string Venue { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Url { get; set; }
        public string TicketMasterId { get; set; } = string.Empty;
        public Guid? DestinationId { get; set; }
    }
}
