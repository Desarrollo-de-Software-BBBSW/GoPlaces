using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;


namespace GoPlaces.Destinations
{
    public class DestinationAppService:
    
    CrudAppService<Destination,
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

        public async Task<DestinationDto> Crear(CreateUpdateDestinationDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                throw new ArgumentException("Destination name cannot be empty");
            }
            var destinationEntity = ObjectMapper.Map<CreateUpdateDestinationDto, Destination>(input);

            var destination = await _repository.InsertAsync(destinationEntity);
            
            return ObjectMapper.Map<Destination, DestinationDto>( destination);
        }
        public async Task<List<DestinationDto>> GetAllDestinationsAsync()
        {
            var destinations = await _repository.GetListAsync();
            return ObjectMapper.Map<List<Destination>, List<DestinationDto>>(destinations);
        }

    }

}
