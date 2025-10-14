using GoPlaces.Destination;
using GoPlaces.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GoPlaces.Destinations
{
    [Collection(GoPlacesTestConsts.CollectionDefinitionName)]
    public class EfCoreDestinationAppService_Tests : DestinationAppService_Tests<GoPlacesEntityFrameworkCoreTestModule>
    {
    }
}
