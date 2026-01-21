using GoPlaces.EntityFrameworkCore;
using GoPlaces.Ratings;
using GoPlaces.Tests.Ratings;
using GoPlaces.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Autofac;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Claims;
using Volo.Abp.SettingManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Volo.Abp.Users;

using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace GoPlaces
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(GoPlacesApplicationModule)
    )]
    public class GoPlacesApplicationTestModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            // 1. Auditoría
            context.Services.Replace(ServiceDescriptor.Singleton<IAuditingStore, NullAuditingStore>());

            // 2. Accessor (Login falso para tests viejos)
            context.Services.Replace(ServiceDescriptor.Singleton<ICurrentPrincipalAccessor, FakeCurrentPrincipalAccessor>());

            Configure<PermissionManagementOptions>(options =>
            {
                options.IsDynamicPermissionStoreEnabled = false;
                options.SaveStaticPermissionsToDatabase = false;
            });

            Configure<SettingManagementOptions>(options =>
            {
                options.IsDynamicSettingStoreEnabled = false;
                options.SaveStaticSettingsToDatabase = false;
            });

            // 3. Repositorios
            context.Services.AddSingleton<IRepository<Rating, Guid>, InMemoryRatingRepository>();

            // 👇 CLAVE: Este es el Mock que le dará datos a tu Perfil. Ya estaba, pero ahora lo usaremos.
            context.Services.AddSingleton(Substitute.For<IIdentityUserRepository>());

            context.Services.AddSingleton(Substitute.For<IIdentityRoleRepository>());
            context.Services.AddSingleton(Substitute.For<IOrganizationUnitRepository>());
            context.Services.AddSingleton(Substitute.For<IIdentityLinkUserRepository>());
            context.Services.AddSingleton(Substitute.For<ISettingDefinitionRecordRepository>());

            // 4. MOCKS PARA LOGIN (Solo SignInManager)

            // Creamos mocks locales solo para poder instanciar SignInManager
            var userStore = Substitute.For<IUserStore<IdentityUser>>();
            var userManager = Substitute.For<UserManager<IdentityUser>>(
                userStore, null, null, null, null, null, null, null, null);

            // NO registramos userManager ni userStore en el sistema global para no confundir a ProfileService.
            // context.Services.AddSingleton(userManager); <--- BORRADO

            var contextAccessor = Substitute.For<IHttpContextAccessor>();
            var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<IdentityUser>>();

            var signInManager = Substitute.For<SignInManager<IdentityUser>>(
                userManager,
                contextAccessor,
                claimsFactory,
                null, null, null, null
            );
            // Este SÍ lo registramos porque LoginAppService lo pide explícitamente
            context.Services.AddSingleton(signInManager);

            // 5. SERVICIOS
            context.Services.AddTransient<IMyRegisterAppService, RegisterAppService>();
            context.Services.AddTransient<IMyLoginAppService, LoginAppService>();
            context.Services.AddTransient<IMyProfileAppService, MyProfileAppService>();
        }
    }

    public class NullAuditingStore : IAuditingStore
    {
        public Task SaveAsync(AuditLogInfo auditInfo)
        {
            return Task.CompletedTask;
        }
    }

    public class FakeCurrentPrincipalAccessor : ICurrentPrincipalAccessor
    {
        public static bool IsAuthenticated { get; set; } = true;

        public ClaimsPrincipal Principal
        {
            get
            {
                if (!IsAuthenticated) return null;
                var claims = new List<Claim>
                {
                    new Claim(AbpClaimTypes.UserId, "2e701e62-0953-4dd3-910b-dc6cc93ccb0d"),
                    new Claim(AbpClaimTypes.UserName, "admin"),
                    new Claim(AbpClaimTypes.Email, "admin@abp.io")
                };
                return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
            }
        }
        public IDisposable Change(ClaimsPrincipal principal) => new DisposeAction(() => { });
    }
}