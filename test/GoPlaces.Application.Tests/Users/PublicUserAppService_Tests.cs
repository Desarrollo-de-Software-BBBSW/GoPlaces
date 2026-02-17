using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data; // 👈 Necesario para SetProperty / GetProperty
using Volo.Abp.Identity;
using Xunit;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace GoPlaces.Users
{
    public class PublicUserAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly IPublicUserLookupAppService _publicAppService;
        private readonly IdentityUserManager _userManager;

        public PublicUserAppService_Tests()
        {
            _publicAppService = GetRequiredService<IPublicUserLookupAppService>();
            _userManager = GetRequiredService<IdentityUserManager>();
        }

        [Fact]
        public async Task GetByUserNameAsync_Should_Return_Public_Profile()
        {
            // 1. ARRANGE
            var targetUserName = "pedro123";
            var photoUrl = "https://pedro.com/foto.jpg";

            await WithUnitOfWorkAsync(async () =>
            {
                // Paso A: Crear el usuario básico primero
                var fakeUser = new IdentityUser(Guid.NewGuid(), targetUserName, "pedro@email.com");
                fakeUser.Name = "Pedro";
                fakeUser.Surname = "Picapiedra";

                (await _userManager.CreateAsync(fakeUser)).CheckErrors();

                // Paso B: AHORA establecemos la propiedad y actualizamos.
                // Esto fuerza a EF Core a detectar el cambio en 'ExtraProperties' y guardar el JSON.
                fakeUser.SetProperty("PhotoUrl", photoUrl);
                (await _userManager.UpdateAsync(fakeUser)).CheckErrors();
            });

            // 2. ACT
            var result = await _publicAppService.GetByUserNameAsync(targetUserName);

            // 3. ASSERT
            result.ShouldNotBeNull();
            result.UserName.ShouldBe(targetUserName);
            result.Name.ShouldBe("Pedro");

            // Ahora sí, el JSON se persistió y la propiedad no será null
            result.PhotoUrl.ShouldBe(photoUrl);
        }

        [Fact]
        public async Task GetByUserNameAsync_Should_Throw_Exception_If_User_Not_Found()
        {
            // 1. ARRANGE
            var targetUserName = "fantasma";

            // 2. ACT & ASSERT
            await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            {
                await _publicAppService.GetByUserNameAsync(targetUserName);
            });
        }
    }
}