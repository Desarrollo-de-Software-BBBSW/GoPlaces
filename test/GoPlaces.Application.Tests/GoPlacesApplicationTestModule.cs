using GoPlaces;
using GoPlaces.EntityFrameworkCore;
using GoPlaces.Ratings;
using GoPlaces.Tests.Ratings;
using GoPlaces.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.Autofac;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Claims;
using Volo.Abp.SettingManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace GoPlaces;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(GoPlacesApplicationModule)
)]
public class GoPlacesApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 1) Auditoría
        context.Services.Replace(ServiceDescriptor.Singleton<IAuditingStore, NullAuditingStore>());

        // 2) Usuario (Login simulado)
        context.Services.Replace(ServiceDescriptor.Singleton<ICurrentPrincipalAccessor, FakeCurrentPrincipalAccessor>());

        // 3) DESHABILITAR CARGA DINÁMICA DE PERMISOS Y SETTINGS (Configuraciones)
        // Esto evita que ABP intente conectarse a la DB para leer reglas.
        Configure<PermissionManagementOptions>(options =>
        {
            options.IsDynamicPermissionStoreEnabled = false;
            options.SaveStaticPermissionsToDatabase = false;
        });

        Configure<SettingManagementOptions>(options => // 👈 NUEVO: Apagamos settings dinámicos
        {
            options.IsDynamicSettingStoreEnabled = false;
            options.SaveStaticSettingsToDatabase = false;
        });

        // 4) Repositorio de Ratings
        context.Services.AddSingleton<IRepository<Rating, Guid>, InMemoryRatingRepository>();

        // 5) MOCKS DE IDENTITY (Para que RegisterAppService arranque)
        context.Services.AddSingleton(Substitute.For<IIdentityUserRepository>());
        context.Services.AddSingleton(Substitute.For<IIdentityRoleRepository>());
        context.Services.AddSingleton(Substitute.For<IOrganizationUnitRepository>());
        context.Services.AddSingleton(Substitute.For<IIdentityLinkUserRepository>());

        // 6) MOCK DE SETTINGS (Solución al error actual) 👈 NUEVO
        context.Services.AddSingleton(Substitute.For<ISettingDefinitionRecordRepository>());

        // 7) Tu servicio a probar
        context.Services.AddTransient<IMyRegisterAppService, RegisterAppService>();

        context.Services.AddTransient<IMyLoginAppService, LoginAppService>();

        // 👇 1. CREAMOS UN MOCK DE USER MANAGER (Necesario para el SignInManager)
        var userStore = Substitute.For<IUserStore<IdentityUser>>();
        var userManager = Substitute.For<UserManager<IdentityUser>>(
            userStore, null, null, null, null, null, null, null, null);
        // 👇 2. CREAMOS UN MOCK DE CONTEXT ACCESSOR
        var contextAccessor = Substitute.For<IHttpContextAccessor>();

        // 👇 3. CREAMOS EL MOCK DE CLAIMS FACTORY
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<IdentityUser>>();

        // 👇 4. FINALMENTE, CREAMOS EL MOCK DE SIGNIN MANAGER
        // SignInManager es una clase concreta, así que usamos Substitute.ForPartsOf
        // para poder sobreescribir sus métodos virtuales (como PasswordSignInAsync)
        var signInManager = Substitute.For<SignInManager<IdentityUser>>(
            userManager,
            contextAccessor,
            claimsFactory,
            null, null, null, null
        );
        // Registramos este mock para que LoginAppService lo use
        context.Services.AddSingleton(signInManager);
    }
}

// --- CLASES AUXILIARES ---

public class NullAuditingStore : IAuditingStore
{
    public Task SaveAsync(AuditLogInfo auditInfo) => Task.CompletedTask;
}

public class FakeCurrentPrincipalAccessor : ICurrentPrincipalAccessor
{
    public static bool IsAuthenticated { get; set; } = true;

    public IDisposable Change(ClaimsPrincipal principal) => new FakeDisposable();

    public ClaimsPrincipal Principal
    {
        get
        {
            if (!IsAuthenticated)
            {
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

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