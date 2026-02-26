using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Security.Claims;
using Xunit;

namespace GoPlaces.ExternalApiMetrics
{
    [Collection(GoPlacesTestConsts.CollectionDefinitionName)]
    public class ExternalApiMetricAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly IExternalApiMetricAppService _metricAppService;
        private readonly IRepository<ExternalApiCall, Guid> _repository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

        public ExternalApiMetricAppService_Tests()
        {
            _metricAppService = GetRequiredService<IExternalApiMetricAppService>();
            _repository = GetRequiredService<IRepository<ExternalApiCall, Guid>>();
            _guidGenerator = GetRequiredService<IGuidGenerator>();
            _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
        }

        // Método mejorado para poder inyectarle el ROL al usuario falso
        private IDisposable ChangeUserContext(Guid userId, string userName, string role)
        {
            var claims = new[]
            {
                new Claim(Volo.Abp.Security.Claims.AbpClaimTypes.UserId, userId.ToString()),
                new Claim(Volo.Abp.Security.Claims.AbpClaimTypes.UserName, userName),
                new Claim(Volo.Abp.Security.Claims.AbpClaimTypes.Role, role) // 👈 Agregamos el ROL
            };
            return _currentPrincipalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));
        }

        [Fact]
        public async Task Should_Get_Metrics_If_Admin()
        {
            var adminId = _guidGenerator.Create();

            // 1. Arrange: Llenamos la BD con 3 llamadas de prueba a una API
            await WithUnitOfWorkAsync(async () =>
            {
                await _repository.InsertAsync(new ExternalApiCall(_guidGenerator.Create(), "Amadeus", "/flights", 200, true));
                await _repository.InsertAsync(new ExternalApiCall(_guidGenerator.Create(), "Amadeus", "/flights", 300, true));
                await _repository.InsertAsync(new ExternalApiCall(_guidGenerator.Create(), "Amadeus", "/hotels", 800, false));
            });

            // 2. Act: Entramos como ADMIN y pedimos las métricas
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(adminId, "superadmin", "admin"))
                {
                    var result = await _metricAppService.GetMetricsAsync();

                    // 3. Assert: Debería calcular todo perfecto
                    result.ShouldNotBeNull();
                    result.Count.ShouldBe(1); // Solo hay un grupo (Amadeus)
                    result[0].ApiName.ShouldBe("Amadeus");
                    result[0].TotalCalls.ShouldBe(3);
                    result[0].SuccessfulCalls.ShouldBe(2);
                    result[0].FailedCalls.ShouldBe(1);
                }
            });
        }

        [Fact]
        public async Task Should_Fail_If_Not_Admin()
        {
            var userId = _guidGenerator.Create();

            await WithUnitOfWorkAsync(async () =>
            {
                // Entramos como un usuario normal ("user")
                using (ChangeUserContext(userId, "lucas", "user"))
                {
                    await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                    {
                        // Intentamos ver las métricas y debe explotar
                        await _metricAppService.GetMetricsAsync();
                    });
                }
            });
        }
    }
}