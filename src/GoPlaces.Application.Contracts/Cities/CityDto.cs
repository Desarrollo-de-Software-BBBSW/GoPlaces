using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoPlaces.Cities
{
    public class CityDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Country { get; set; }
    }

}
