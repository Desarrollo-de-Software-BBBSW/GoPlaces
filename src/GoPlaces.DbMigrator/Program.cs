using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;

namespace GoPlaces.DbMigrator;

class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
#if DEBUG
                .MinimumLevel.Override("GoPlaces", LogEventLevel.Debug)
#else
                .MinimumLevel.Override("GoPlaces", LogEventLevel.Information)
#endif
                .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        await CreateHostBuilder(args).RunConsoleAsync();
    }


public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            var env = hostingContext.HostingEnvironment;

            // Fuerza a leer estos archivos desde el proyecto DbMigrator
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                  .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                  .AddEnvironmentVariables();

            // Si querés, podés permitir sobreescribir por línea de comandos:
            // dotnet run --project ... -- ConnectionStrings:Default="..."
            config.AddCommandLine(args);
        })
        .AddAppSettingsSecretsJson()
        .ConfigureLogging((context, logging) => logging.ClearProviders())
        .ConfigureServices((hostContext, services) =>
        {
            services.AddHostedService<DbMigratorHostedService>();
        });

}
