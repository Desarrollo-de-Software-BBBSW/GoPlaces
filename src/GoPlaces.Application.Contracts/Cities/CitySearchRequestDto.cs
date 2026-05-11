using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoPlaces.Cities
{
    public class CitySearchRequestDto
    {
        public string? PartialName { get; set; }

        /// <summary>Código ISO del país (ej: "US", "AR"). Opcional.</summary>
        public string? CountryCode { get; set; }

        /// <summary>Código de región/provincia dentro del país. Opcional.</summary>
        public string? RegionId { get; set; }

        /// <summary>Población mínima de la ciudad. Debe ser mayor o igual a 0.</summary>
        public int? MinPopulation { get; set; }
    }

}
