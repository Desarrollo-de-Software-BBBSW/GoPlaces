using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public Coordinates Coordinates { get; private set; } = default!;

    private Destination() { } // EF Core

    public Destination(
        Guid id,
        string name,
        string country,
        long population,
        Coordinates coordinates,
        string? imageUrl = null,
        DateTime? lastUpdatedDate = null) : base(id)
    {
        SetName(name);
        SetCountry(country);
        SetPopulation(population);
        SetCoordinates(coordinates);
        SetImageUrl(imageUrl);
        SetLastUpdatedDate(lastUpdatedDate ?? DateTime.UtcNow);
    }

    public void SetName(string name) =>
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: NameMaxLength);

    public void SetCountry(string country) =>
        Country = Check.NotNullOrWhiteSpace(country, nameof(country), maxLength: CountryMaxLength);

    public void SetPopulation(long population)
    {
        if (population < 0)
            throw new BusinessException("GoPlaces:InvalidPopulation");
        Population = population;
    }

    public void SetImageUrl(string? imageUrl)
    {
        if (!string.IsNullOrWhiteSpace(imageUrl))
            Check.Length(imageUrl, nameof(imageUrl), maxLength: ImageUrlMaxLength);
        ImageUrl = imageUrl?.Trim();
    }

    public void SetLastUpdatedDate(DateTime when)
    {
        if (when == default) when = DateTime.UtcNow;
        LastUpdatedDate = when;
    }

    public void SetCoordinates(Coordinates coordinates) =>
        Coordinates = Check.NotNull(coordinates, nameof(coordinates));
}
