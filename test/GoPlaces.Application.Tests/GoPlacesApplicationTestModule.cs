using GoPlaces;
using GoPlaces.EntityFrameworkCore;
using GoPlaces.Ratings;
using GoPlaces.Tests.Ratings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.Autofac;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Claims;

namespace GoPlaces;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(GoPlacesApplicationModule)
)]
public class GoPlacesApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 1) Solución Auditoría (Evita error de IAuditLogRepository)
        context.Services.Replace(ServiceDescriptor.Singleton<IAuditingStore, NullAuditingStore>());

        // 2) Solución Usuario (Permite simular login/logout en los tests)
        context.Services.Replace(ServiceDescriptor.Singleton<ICurrentPrincipalAccessor, FakeCurrentPrincipalAccessor>());

        // 3) Deshabilitar permisos dinámicos
        Configure<PermissionManagementOptions>(options =>
        {
            options.IsDynamicPermissionStoreEnabled = false;
            options.SaveStaticPermissionsToDatabase = false;
        });

        // 4) Repositorio en memoria
        context.Services.AddSingleton<IRepository<Rating, Guid>, InMemoryRatingRepository>();
    }
}

// --- CLASES AUXILIARES ---

public class NullAuditingStore : IAuditingStore
{
    public Task SaveAsync(AuditLogInfo auditInfo) => Task.CompletedTask;
}

public class FakeCurrentPrincipalAccessor : ICurrentPrincipalAccessor
{
    // Bandera estática para controlar el login desde el test
    public static bool IsAuthenticated { get; set; } = true;

    public IDisposable Change(ClaimsPrincipal principal) => new FakeDisposable();

    public ClaimsPrincipal Principal
    {
        get
        {
            if (!IsAuthenticated)
            {
                // Retorna identidad anónima
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            // Retorna identidad admin
            var claims = new List<Claim>
            {
                new Claim(AbpClaimTypes.UserId, "2e701e62-0953-4dd3-910b-dc6cc93ccb0d"),
                new Claim(AbpClaimTypes.UserName, "admin"),
                new Claim(AbpClaimTypes.Email, "admin@goplaces.com")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            return new ClaimsPrincipal(identity);
        }
    }
}

public class FakeDisposable : IDisposable
{
    public void Dispose() { }
}