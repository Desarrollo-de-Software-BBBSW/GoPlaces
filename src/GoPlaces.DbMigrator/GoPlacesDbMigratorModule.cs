using GoPlaces.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace GoPlaces.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(GoPlacesEntityFrameworkCoreModule),
    typeof(GoPlacesApplicationContractsModule)
)]
public class GoPlacesDbMigratorModule : AbpModule
{
}
