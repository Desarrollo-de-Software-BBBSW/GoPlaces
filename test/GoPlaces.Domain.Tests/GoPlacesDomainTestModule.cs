using Volo.Abp.Modularity;

namespace GoPlaces;

[DependsOn(
    typeof(GoPlacesDomainModule),
    typeof(GoPlacesTestBaseModule)
)]
public class GoPlacesDomainTestModule : AbpModule
{

}
