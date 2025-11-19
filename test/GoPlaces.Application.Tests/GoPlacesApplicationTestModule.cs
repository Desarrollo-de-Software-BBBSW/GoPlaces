using GoPlaces;
using GoPlaces.EntityFrameworkCore;   // 👈 IMPORTANTE
using GoPlaces.Ratings;
using GoPlaces.Tests.Ratings;
using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp.Autofac;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Authorization;


namespace GoPlaces;

// Este módulo de tests DEBE depender del ApplicationModule
[DependsOn(
    typeof(AbpAutofacModule),
    typeof(GoPlacesApplicationModule)
)]
public class GoPlacesApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
       

        // 2) Deshabilitar el store dinámico de permisos (no usamos DB en tests)
        Configure<PermissionManagementOptions>(options =>
        {
            options.IsDynamicPermissionStoreEnabled = false;
            options.SaveStaticPermissionsToDatabase = false;
        });

        // 3) Registrar tu repositorio en memoria para Rating
        context.Services.AddSingleton<IRepository<Rating, Guid>, InMemoryRatingRepository>();
    }
}
