using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace GoPlaces.Destinations;

public class Destination : FullAuditedAggregateRoot<Guid>
{
    public const int NameMaxLength = 128;
    public const int CountryMaxLength = 64;
    public const int ImageUrlMaxLength = 512;

    public string Name { get; private set; }
    public string Country { get; private set; }
    public long Population { get; private set; }
    public string? ImageUrl { get; private set; }
    public DateTime LastUpdatedDate { get; private set; }
    public Coordinates Coordinates { get; private set; }

    private Destination() { }

    public Destination(Guid id, string name, string country, long population, Coordinates coordinates, string? imageUrl = null, DateTime? lastUpdatedDate = null)
        : base(id)
    {
        SetName(name);
        SetCountry(country);
        SetPopulation(population);
        SetCoordinates(coordinates);
        SetImageUrl(imageUrl);
        SetLastUpdatedDate(lastUpdatedDate ?? DateTime.UtcNow);
    }

    public void SetName(string name) => Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: NameMaxLength);
    public void SetCountry(string country) => Country = Check.NotNullOrWhiteSpace(country, nameof(country), maxLength: CountryMaxLength);
    public void SetPopulation(long population) => Population = population >= 0 ? population : throw new BusinessException("GoPlaces:InvalidPopulation");

    // 👇 ESTE ES EL QUE TE FALTA Y ROMPE LA COMPILACIÓN
    public void SetCoordinates(Coordinates coordinates) => Coordinates = Check.NotNull(coordinates, nameof(coordinates));

    public void SetImageUrl(string? imageUrl) => ImageUrl = imageUrl?.Trim();
    public void SetLastUpdatedDate(DateTime when) => LastUpdatedDate = when == default ? DateTime.UtcNow : when;
}