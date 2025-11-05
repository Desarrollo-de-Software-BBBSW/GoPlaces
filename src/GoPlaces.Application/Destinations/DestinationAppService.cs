using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace GoPlaces.Destinations
{
    public class DestinationAppService :
        CrudAppService<
            Destination,
            DestinationDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateDestinationDto>,
        IDestinationAppService
    {
        private readonly IRepository<Destination, Guid> _repository;

        public DestinationAppService(IRepository<Destination, Guid> repository)
            : base(repository)
        {
            _repository = repository;
        }

        // ✅ Crear usando AutoMapper (usa AfterMap para SetCoordinates)
        public async Task<DestinationDto> Crear(CreateUpdateDestinationDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
                throw new ArgumentException("Destination name cannot be empty");

            // Mapea DTO -> Entity (Coordinates se setea en AfterMap del perfil)
            var entity = ObjectMapper.Map<CreateUpdateDestinationDto, Destination>(input);

            // Insert y autosave para que quede persistido inmediatamente
            entity = await _repository.InsertAsync(entity, autoSave: true);

            return ObjectMapper.Map<Destination, DestinationDto>(entity);
        }

        // Opción A (usa AutoMapper) — si ya corregiste el perfil con Latitude/Longitude
        public async Task<List<DestinationDto>> GetAllDestinationsAsync()
        {
            var destinations = await _repository.GetListAsync();
            return ObjectMapper.Map<List<Destination>, List<DestinationDto>>(destinations);
        }

        /* 
        // Opción B (proyección manual a DTO, si preferís no depender del mapper acá)
        public async Task<List<DestinationDto>> GetAllDestinationsAsync()
        {
            var query = await _repository.GetQueryableAsync();

            var list = await query
                .Select(d => new DestinationDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Country = d.Country,
                    Population = d.Population,
                    ImageUrl = d.ImageUrl,
                    LastUpdatedDate = d.LastUpdatedDate,
                    Latitude = d.Coordinates.Latitude,
                    Longitude = d.Coordinates.Longitude
                })
                .ToListAsync();

            return list;
        }
        */
    }
}
