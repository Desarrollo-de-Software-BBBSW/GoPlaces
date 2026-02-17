using GoPlaces.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Security.Claims;
using System.Threading;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Autofac;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Claims;
using Volo.Abp.SettingManagement;
using Volo.Abp.Uow;

// Alias para resolver ambigüedad CS0104
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace GoPlaces;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(GoPlacesApplicationModule),
    typeof(GoPlacesEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
)]
public class GoPlacesApplicationTestModule : AbpModule
{
    private SqliteConnection? _sqliteConnection;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        _sqliteConnection = CreateDatabaseAndGetConnection();

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(ctx => ctx.DbContextOptions.UseSqlite(_sqliteConnection));
        });

        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        context.Services.AddAbpDbContext<GoPlacesDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        context.Services.Replace(ServiceDescriptor.Singleton<IAuditingStore, NullAuditingStore>());
        context.Services.Replace(ServiceDescriptor.Singleton<ICurrentPrincipalAccessor, FakeCurrentPrincipalAccessor>());
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
            context.Database.EnsureCreated();
        }

        return connection;
    }
}

public class NullAuditingStore : IAuditingStore
{
    public System.Threading.Tasks.Task SaveAsync(AuditLogInfo auditInfo) => System.Threading.Tasks.Task.CompletedTask;
}

public class FakeCurrentPrincipalAccessor : ICurrentPrincipalAccessor
{
    private readonly AsyncLocal<ClaimsPrincipal> _currentPrincipal = new AsyncLocal<ClaimsPrincipal>();
    public ClaimsPrincipal Principal => _currentPrincipal.Value ?? GetDefaultPrincipal();

    private ClaimsPrincipal GetDefaultPrincipal()
    {
        return new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(AbpClaimTypes.UserId, Guid.NewGuid().ToString()),
            new Claim(AbpClaimTypes.UserName, "admin")
        }, "Tests"));
    }

    public IDisposable Change(ClaimsPrincipal principal)
    {
        var parent = Principal;
        _currentPrincipal.Value = principal;
        return new DisposeAction(() => { _currentPrincipal.Value = parent; });
    }
}