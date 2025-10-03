using Volo.Abp.Modularity;

namespace GoPlaces;

public abstract class GoPlacesApplicationTestBase<TStartupModule> : GoPlacesTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
