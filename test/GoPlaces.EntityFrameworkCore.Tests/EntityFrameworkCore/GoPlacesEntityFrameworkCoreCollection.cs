using Xunit;

namespace GoPlaces.EntityFrameworkCore;

[CollectionDefinition(GoPlacesTestConsts.CollectionDefinitionName)]
public class GoPlacesEntityFrameworkCoreCollection : ICollectionFixture<GoPlacesEntityFrameworkCoreFixture>
{

}
