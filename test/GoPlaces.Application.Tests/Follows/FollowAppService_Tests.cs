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
    [Collection(GoPlacesTestConsts.CollectionDefinitionName)]
    public class FollowAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly IFollowAppService _followAppService;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;
        private readonly IFollowListRepository _followListRepo;

        public FollowAppService_Tests()
        {
            _followAppService = GetRequiredService<IFollowAppService>();
            _guidGenerator = GetRequiredService<IGuidGenerator>();
            _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();

            // Inyectamos el repo de tu amigo
            _followListRepo = GetRequiredService<IFollowListRepository>();
        }

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
                    var result = await _followAppService.SaveDestinationAsync(input);

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
                    await _followAppService.SaveDestinationAsync(input);

                    await Assert.ThrowsAsync<BusinessException>(async () =>
                    {
                        await _followAppService.SaveDestinationAsync(input);
                    });
                }
            });
        }

        [Fact]
        public async Task Should_Remove_Destination_If_Owner()
        {
            var userId = _guidGenerator.Create();
            var destinationId = _guidGenerator.Create();
            var input = new SaveOrRemoveInputDto { DestinationId = destinationId };

            // 1. El usuario guarda el destino
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(userId, "traveler_lucas"))
                {
                    await _followAppService.SaveDestinationAsync(input);
                }
            });

            // 2. El usuario elimina el destino
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(userId, "traveler_lucas"))
                {
                    await _followAppService.RemoveDestinationAsync(input);
                }
            });

            // 3. Verificamos que realmente se haya borrado
            await WithUnitOfWorkAsync(async () =>
            {
                var list = await _followListRepo.FindDefaultByOwnerAsync(userId);

                list.ShouldNotBeNull();
                list.HasDestination(destinationId).ShouldBeFalse();
            });
        }

        [Fact]
        public async Task Should_Fail_Remove_If_Not_Owner()
        {
            var ownerId = _guidGenerator.Create();
            var hackerId = _guidGenerator.Create();
            var destinationId = _guidGenerator.Create();
            var input = new SaveOrRemoveInputDto { DestinationId = destinationId };

            // 1. El dueño guarda el destino en su cuenta
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(ownerId, "traveler_lucas"))
                {
                    await _followAppService.SaveDestinationAsync(input);
                }
            });

            // 2. Un hacker intenta borrar el mismo destino
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(hackerId, "hacker_lucas"))
                {
                    await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    {
                        await _followAppService.RemoveDestinationAsync(input);
                    });
                }
            });

            // 3. Verificamos que el destino SIGUE estando a salvo
            await WithUnitOfWorkAsync(async () =>
            {
                var list = await _followListRepo.FindDefaultByOwnerAsync(ownerId);

                list.ShouldNotBeNull();
                list.HasDestination(destinationId).ShouldBeTrue();
            });
        }

        // 👇 NUEVA PRUEBA: Consultar la lista personal de favoritos
        [Fact]
        public async Task Should_Get_My_Favorites()
        {
            var userId = _guidGenerator.Create();
            var destinationId1 = _guidGenerator.Create();
            var destinationId2 = _guidGenerator.Create();

            // 1. El usuario guarda DOS destinos diferentes
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(userId, "traveler_lucas"))
                {
                    await _followAppService.SaveDestinationAsync(new SaveOrRemoveInputDto { DestinationId = destinationId1 });
                    await _followAppService.SaveDestinationAsync(new SaveOrRemoveInputDto { DestinationId = destinationId2 });
                }
            });

            // 2. El usuario consulta su lista
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(userId, "traveler_lucas"))
                {
                    var result = await _followAppService.GetMyFavoritesAsync();

                    // 3. Verificamos que traiga exactamente 2 y que sean los correctos
                    result.ShouldNotBeNull();
                    result.Count.ShouldBe(2);
                    result.ShouldContain(x => x.DestinationId == destinationId1);
                    result.ShouldContain(x => x.DestinationId == destinationId2);
                }
            });
        }
    }
}