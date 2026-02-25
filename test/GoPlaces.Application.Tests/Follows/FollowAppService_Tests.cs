using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Security.Claims;
using Volo.Abp.Guids;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace GoPlaces.Follows
{
    public class FollowAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly IFollowAppService _followAppService;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

        public FollowAppService_Tests()
        {
            _followAppService = GetRequiredService<IFollowAppService>();
            _guidGenerator = GetRequiredService<IGuidGenerator>();
            _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
        }

        // Simula a un usuario logueado
        private IDisposable ChangeUserContext(Guid userId, string userName)
        {
            var claims = new[]
            {
                new Claim(AbpClaimTypes.UserId, userId.ToString()),
                new Claim(AbpClaimTypes.UserName, userName)
            };
            return _currentPrincipalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));
        }

        [Fact]
        public async Task Should_Create_List_And_Save_Destination()
        {
            var userId = _guidGenerator.Create();
            var destinationId = _guidGenerator.Create();

            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(userId, "traveler_lucas"))
                {
                    var input = new SaveOrRemoveInputDto { DestinationId = destinationId };

                    // Act
                    var result = await _followAppService.SaveDestinationAsync(input);

                    // Assert
                    result.ShouldNotBeNull();
                    result.DestinationId.ShouldBe(destinationId);
                }
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_If_Already_Saved()
        {
            var userId = _guidGenerator.Create();
            var destinationId = _guidGenerator.Create();

            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(userId, "traveler_lucas"))
                {
                    var input = new SaveOrRemoveInputDto { DestinationId = destinationId };

                    // Guardamos por primera vez (Funciona)
                    await _followAppService.SaveDestinationAsync(input);

                    // Guardamos por segunda vez (Debe lanzar BusinessException)
                    await Assert.ThrowsAsync<BusinessException>(async () =>
                    {
                        await _followAppService.SaveDestinationAsync(input);
                    });
                }
            });
        }
    }
}