using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace GoPlaces.Follows;

public class SavedDestinationDto : EntityDto<Guid>
{
    public Guid DestinationId { get; set; }
    public DateTime CreationTime { get; set; }
}
