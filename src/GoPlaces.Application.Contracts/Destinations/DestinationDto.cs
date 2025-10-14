using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace GoPlaces.Destinations;

public class DestinationDto : EntityDto<Guid>
{
    public string Name { get; set; } = default!;
    public string Country { get; set; } = default!;
    public long Population { get; set; }
    public string? ImageUrl { get; set; }          // Url_Image
    public DateTime LastUpdatedDate { get; set; }  // last_updated_date
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
