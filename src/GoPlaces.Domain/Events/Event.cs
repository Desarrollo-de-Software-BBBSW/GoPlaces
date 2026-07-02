using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace GoPlaces.Events;

public class Event : FullAuditedAggregateRoot<Guid>
{
    public const int NameMaxLength = 256;
    public const int VenueMaxLength = 256;
    public const int CityMaxLength = 128;
    public const int UrlMaxLength = 1024;
    public const int TicketMasterIdMaxLength = 64;

    public string Name { get; private set; }
    public string? Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public string Venue { get; private set; }
    public string City { get; private set; }
    public string? Url { get; private set; }
    public string TicketMasterId { get; private set; }
    public Guid? DestinationId { get; private set; }

    private Event() { }

    public Event(
        Guid id,
        string name,
        DateTime startDate,
        string venue,
        string city,
        string ticketMasterId,
        string? description = null,
        string? url = null,
        Guid? destinationId = null)
        : base(id)
    {
        SetName(name);
        SetStartDate(startDate);
        SetVenue(venue);
        SetCity(city);
        SetTicketMasterId(ticketMasterId);
        SetDescription(description);
        SetUrl(url);
        SetDestinationId(destinationId);
    }

    public void SetName(string name) => Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: NameMaxLength);
    public void SetDescription(string? description) => Description = description?.Trim();
    public void SetStartDate(DateTime startDate) => StartDate = startDate;
    public void SetVenue(string venue) => Venue = Check.NotNullOrWhiteSpace(venue, nameof(venue), maxLength: VenueMaxLength);
    public void SetCity(string city) => City = Check.NotNullOrWhiteSpace(city, nameof(city), maxLength: CityMaxLength);
    public void SetUrl(string? url) => Url = url?.Trim();
    public void SetTicketMasterId(string ticketMasterId) => TicketMasterId = Check.NotNullOrWhiteSpace(ticketMasterId, nameof(ticketMasterId), maxLength: TicketMasterIdMaxLength);
    public void SetDestinationId(Guid? destinationId) => DestinationId = destinationId;
}
