using AutoMapper;
using GoPlaces.Destinations;

namespace GoPlaces;

public class GoPlacesApplicationAutoMapperProfile : Profile
{
    public GoPlacesApplicationAutoMapperProfile()
    {
        CreateMap<Destination, DestinationDto>()
            .ForMember(d => d.Latitude, opt => opt.MapFrom(s => s.Coordinates.Latitude))
            .ForMember(d => d.Longitude, opt => opt.MapFrom(s => s.Coordinates.Longitude));
        // Name, Country, Population, ImageUrl, LastUpdatedDate se mapean por convención.
        CreateMap<Destinations.CreateUpdateDestinationDto, Destinations.Destination>()
            .ForMember(d => d.Coordinates, opt => opt.MapFrom(s => new Coordinates(s.Latitude, s.Longitude)));
    }
}
