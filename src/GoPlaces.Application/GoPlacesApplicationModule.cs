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

namespace GoPlaces
{
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
            // Configuración de AutoMapper
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<GoPlacesApplicationModule>();
            });

            // 1️⃣ Registra el servicio de búsqueda de ciudades
            context.Services.AddScoped<ICitySearchService, GeoDbCitySearchService>();

            // 2️⃣ Configura el HttpClient nombrado "GeoDB"
            context.Services.AddHttpClient("GeoDB", (sp, client) =>
            {
                // Obtenemos la configuración del contenedor de dependencias
                var configuration = sp.GetRequiredService<IConfiguration>();
                var apiKey = configuration["RapidApi:ApiKey"];

                // Endpoint base de la API GeoDB Cities
                client.BaseAddress = new Uri("https://wft-geo-db.p.rapidapi.com/v1/geo/");

                // Headers requeridos por RapidAPI
                client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "wft-geo-db.p.rapidapi.com");

                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
                }
            });
        }
    }
}
