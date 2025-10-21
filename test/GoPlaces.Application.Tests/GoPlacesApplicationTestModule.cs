using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace GoPlaces
{
    // Este módulo de tests DEBE depender del ApplicationModule
    [DependsOn(
        typeof(AbpAutofacModule),        // Usa Autofac en el host de pruebas
        typeof(GoPlacesApplicationModule)// <<-- IMPORTANTE
    )]
    public class GoPlacesApplicationTestModule : AbpModule
    {
    }
}
