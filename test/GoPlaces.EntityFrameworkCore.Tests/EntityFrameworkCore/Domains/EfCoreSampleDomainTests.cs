using GoPlaces.Samples;
using Xunit;

namespace GoPlaces.EntityFrameworkCore.Domains;

[Collection(GoPlacesTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<GoPlacesEntityFrameworkCoreTestModule>
{

}
