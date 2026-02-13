using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos; // Recomendado para DTOs

namespace GoPlaces.Cities
{
    // ✅ CAMBIO 1: Heredar de EntityDto<Guid> simplifica las cosas (ya trae el Id)
    // O puedes dejarlo como "public class CityDto" y solo cambiar el tipo de Id.
    public class CityDto : EntityDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        // Esta propiedad es vital para las estrellitas
        public double Rating { get; set; }
    }
}