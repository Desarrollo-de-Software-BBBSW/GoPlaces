using GoPlaces;
using GoPlaces.EntityFrameworkCore;
using GoPlaces.Ratings;
using GoPlaces.Tests.Ratings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions; // Necesario para Replace
using System;
using System.Collections.Generic;
using System.Security.Claims; // Necesario para Claims
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.Autofac;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Claims; // Necesario para ICurrentPrincipalAccessor y AbpClaimTypes
using Volo.Abp.Authorization;

namespace GoPlaces;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(GoPlacesApplicationModule)
)]
public class GoPlacesApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 1) Solución al error de Auditoría (IAuditLogRepository)
        context.Services.Replace(ServiceDescriptor.Singleton<IAuditingStore, NullAuditingStore>());

        // 2) Solución al error de Usuario Nulo (CurrentUser.Id es null)
        // Reemplazamos el "Lector de Usuario" por uno que siempre devuelve un usuario fijo.
        context.Services.Replace(ServiceDescriptor.Singleton<ICurrentPrincipalAccessor, FakeCurrentPrincipalAccessor>());

        // 3) Deshabilitar el store dinámico de permisos
        Configure<PermissionManagementOptions>(options =>
        {
            options.IsDynamicPermissionStoreEnabled = false;
            options.SaveStaticPermissionsToDatabase = false;
        });

        // 4) Registrar tu repositorio en memoria para Rating
        context.Services.AddSingleton<IRepository<Rating, Guid>, InMemoryRatingRepository>();
    }
}

// --- CLASES AUXILIARES PARA LAS PRUEBAS ---

// Evita el error de "Cannot resolve IAuditLogRepository"
public class NullAuditingStore : IAuditingStore
{
    public Task SaveAsync(AuditLogInfo auditInfo)
    {
        return Task.CompletedTask;
    }
}

// Simula un usuario logueado para evitar "Nullable object must have a value" en CurrentUser.Id
public class FakeCurrentPrincipalAccessor : ICurrentPrincipalAccessor
{
    public IDisposable Change(ClaimsPrincipal principal)
    {
        // Retorna un disposable vacío, ya que no necesitamos cambiar de usuario en estas pruebas
        return new FakeDisposable();
    }

    public ClaimsPrincipal Principal
    {
        get
        {
            // Creamos un usuario ficticio con un ID fijo y Rol de Admin
            var claims = new List<Claim>
            {
                new Claim(AbpClaimTypes.UserId, "2e701e62-0953-4dd3-910b-dc6cc93ccb0d"),
                new Claim(AbpClaimTypes.UserName, "admin"),
                new Claim(AbpClaimTypes.Email, "admin@goplaces.com")
            };
            var identity = new ClaimsIdentity(claims, "Test"); // "Test" es el tipo de autenticación
            return new ClaimsPrincipal(identity);
        }
    }
}

// Helper simple para el IDisposable
public class FakeDisposable : IDisposable
{
    public void Dispose() { }
}