using GoPlaces.Samples;
using Xunit;

namespace GoPlaces.EntityFrameworkCore.Applications;

[Collection(GoPlacesTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<GoPlacesEntityFrameworkCoreTestModule>
{

}
