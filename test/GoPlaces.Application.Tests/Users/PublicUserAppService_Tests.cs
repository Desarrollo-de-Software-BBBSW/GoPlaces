using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using NSubstitute;
using Volo.Abp.Identity;
using Volo.Abp.Data; // Para SetProperty
using Volo.Abp;

namespace GoPlaces.Users
{
    public class PublicUserAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly IPublicUserAppService _publicAppService;
        private readonly IIdentityUserRepository _fakeUserRepository;

        public PublicUserAppService_Tests()
        {
            _publicAppService = GetRequiredService<IPublicUserAppService>();
            _fakeUserRepository = GetRequiredService<IIdentityUserRepository>();
        }

        [Fact]
        public async Task GetByUserNameAsync_Should_Return_Public_Profile()
        {
            // 1. ARRANGE
            var targetUserName = "pedro123";
            var normalizedUserName = "PEDRO123"; // Identity busca por nombre normalizado (Mayúsculas)

            var fakeUser = new IdentityUser(Guid.NewGuid(), targetUserName, "pedro@email.com");
            fakeUser.Name = "Pedro";
            fakeUser.Surname = "Picapiedra";
            fakeUser.SetProperty("PhotoUrl", "https://pedro.com/foto.jpg");

            // Simulamos que el repositorio encuentra al usuario cuando buscamos por su nombre normalizado
            _fakeUserRepository.FindByNormalizedUserNameAsync(normalizedUserName, Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(fakeUser));

            // 2. ACT
            var result = await _publicAppService.GetByUserNameAsync(targetUserName);

            // 3. ASSERT
            result.ShouldNotBeNull();
            result.UserName.ShouldBe(targetUserName);
            result.Name.ShouldBe("Pedro");
            result.PhotoUrl.ShouldBe("https://pedro.com/foto.jpg");

            // Verificamos que NO estamos exponiendo datos sensibles (aunque el DTO ni siquiera tiene la propiedad Email, es bueno recordarlo)
            // (El compilador daría error si intentas result.Email, lo cual es perfecto)
        }

        [Fact]
        public async Task GetByUserNameAsync_Should_Throw_Exception_If_User_Not_Found()
        {
            // 1. ARRANGE
            var targetUserName = "fantasma";
            var normalizedUserName = "FANTASMA";

            // El repo devuelve null
            _fakeUserRepository.FindByNormalizedUserNameAsync(normalizedUserName, Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult((IdentityUser)null));

            // 2. ACT & ASSERT
            await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            {
                await _publicAppService.GetByUserNameAsync(targetUserName);
            });
        }
    }
}