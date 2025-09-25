using Volo.Abp.Modularity;

namespace GoPlaces;

[DependsOn(
    typeof(GoPlacesApplicationModule),
    typeof(GoPlacesDomainTestModule)
)]
public class GoPlacesApplicationTestModule : AbpModule
{

}
