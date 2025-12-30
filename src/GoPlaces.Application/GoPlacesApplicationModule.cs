using GoPlaces.Cities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp.Account;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;

namespace GoPlaces
{
    [DependsOn(
        typeof(GoPlacesDomainModule),
        typeof(GoPlacesApplicationContractsModule),
        typeof(AbpPermissionManagementApplicationModule),
        typeof(AbpFeatureManagementApplicationModule),
        typeof(AbpIdentityApplicationModule),
        typeof(AbpAccountApplicationModule),
        typeof(AbpSettingManagementApplicationModule),    
        typeof(AbpDddApplicationModule),
        typeof(AbpAutoMapperModule),            // 👈 necesario
        typeof(GoPlacesApplicationContractsModule)
    )]
    public class GoPlacesApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            // 👇 REGISTRA el object mapper de este módulo
            context.Services.AddAutoMapperObjectMapper<GoPlacesApplicationModule>();

            // 👇 Carga TODOS los perfiles del ensamblado Application (incluye GoPlacesApplicationAutoMapperProfile)
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<GoPlacesApplicationModule>(validate:false);
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
