using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;

namespace GoPlaces.Destinations
{
    public interface IDestinationAppService:
    
        ICrudAppService<
            DestinationDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateDestinationDto>
    {
        Task<List<DestinationDto>> GetAllDestinationsAsync();
        Task<DestinationDto> Crear(CreateUpdateDestinationDto input);
    }
}
