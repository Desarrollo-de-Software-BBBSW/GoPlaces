using GoPlaces.Cities;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Microsoft.Extensions.Configuration;
using System;

namespace GoPlaces;

[DependsOn(
    typeof(GoPlacesDomainModule),
    typeof(GoPlacesApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class GoPlacesApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<GoPlacesApplicationModule>();
        });

        // 1. Registra tu servicio especialista
        context.Services.AddScoped<ICitySearchService, GeoDbCitySearchService>();

        // 2. Configura el HttpClient
        context.Services.AddHttpClient("GeoDB", client =>
        {
            client.BaseAddress = new Uri("https://wft-geo-db.p.rapidapi.com/v1/geo/");

            var configuration = context.Services.GetRequiredService<IConfiguration>();
            var apiKey = configuration["RapidApi:ApiKey"];

            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "wft-geo-db.p.rapidapi.com");
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
        });

    }
}
