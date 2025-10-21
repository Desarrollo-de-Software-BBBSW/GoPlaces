using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Autofac;                 // ✅ Autofac para el host de pruebas
using Volo.Abp.Data;                    // ✅ Para IDataSeeder
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace GoPlaces.EntityFrameworkCore
{
    [DependsOn(
        typeof(AbpAutofacModule),                 // ✅
        typeof(GoPlacesEntityFrameworkCoreModule),// ✅ capa EFCore real
        typeof(GoPlacesTestBaseModule)            // ✅ base de tests (no Application.Tests)
    )]
    public class GoPlacesEntityFrameworkCoreTestModule : AbpModule
    {
        private SqliteConnection? _sqliteConnection;

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<FeatureManagementOptions>(options =>
            {
                options.SaveStaticFeaturesToDatabase = false;
                options.IsDynamicFeatureStoreEnabled = false;
            });
            Configure<PermissionManagementOptions>(options =>
            {
                options.SaveStaticPermissionsToDatabase = false;
                options.IsDynamicPermissionStoreEnabled = false;
            });

            // En tests deshabilitamos transacciones de UoW para Sqlite in-memory
            context.Services.AddAlwaysDisableUnitOfWorkTransaction();

            ConfigureInMemorySqlite(context.Services);
        }

        // 🔑 Ejecutar seeding al iniciar el host de pruebas (crea admin, roles, etc.)
        public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
        {
            var seeder = context.ServiceProvider.GetRequiredService<IDataSeeder>();
            await seeder.SeedAsync();
        }

        private void ConfigureInMemorySqlite(IServiceCollection services)
        {
            _sqliteConnection = CreateDatabaseAndGetConnection();

            services.Configure<AbpDbContextOptions>(options =>
            {
                options.Configure(context =>
                {
                    context.DbContextOptions.UseSqlite(_sqliteConnection);
                });
            });
        }

        public override void OnApplicationShutdown(ApplicationShutdownContext context)
        {
            _sqliteConnection?.Dispose();
        }

        private static SqliteConnection CreateDatabaseAndGetConnection()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<GoPlacesDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var context = new GoPlacesDbContext(options))
            {
                context.GetService<IRelationalDatabaseCreator>().CreateTables();
            }

            return connection;
        }
    }
}
