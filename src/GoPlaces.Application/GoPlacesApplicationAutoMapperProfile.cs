using AutoMapper;
using GoPlaces.Destinations;
using GoPlaces.Ratings;
using GoPlaces.Experiences;

namespace GoPlaces;

public class GoPlacesApplicationAutoMapperProfile : Profile
{
    public GoPlacesApplicationAutoMapperProfile()
    {
        // Entity -> DTO (aplana Coordinates)
        CreateMap<Destination, DestinationDto>()
            .ForMember(d => d.Latitude, opt => opt.MapFrom(s => s.Coordinates.Latitude))
            .ForMember(d => d.Longitude, opt => opt.MapFrom(s => s.Coordinates.Longitude));
        // Name, Country, Population, ImageUrl, LastUpdatedDate se mapean por convención.

        // DTO -> Entity (ignora Coordinates y lo setea en AfterMap con tu método de dominio)
        CreateMap<CreateUpdateDestinationDto, Destination>()
            .ForMember(d => d.Coordinates, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                dest.SetCoordinates(new Coordinates(src.Latitude, src.Longitude));
            });

        // Ratings
        CreateMap<Rating, RatingDto>();

        CreateMap<Experience, ExperienceDto>();
        CreateMap<CreateUpdateExperienceDto, Experience>();
    }
}


