using Volo.Abp.Modularity;

namespace GoPlaces;

/* Inherit from this class for your domain layer tests. */
public abstract class GoPlacesDomainTestBase<TStartupModule> : GoPlacesTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
