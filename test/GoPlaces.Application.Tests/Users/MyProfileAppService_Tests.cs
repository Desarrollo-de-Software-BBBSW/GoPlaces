using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using NSubstitute;
using Volo.Abp.Identity;
using GoPlaces.Users;
using Volo.Abp.Data;

using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace GoPlaces.Tests.Users
{
    public class MyProfileAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly IMyProfileAppService _profileAppService;

        // 👇 Ahora trabajamos con el Repositorio, que es mucho más fiable
        private readonly IIdentityUserRepository _fakeUserRepository;

        public MyProfileAppService_Tests()
        {
            _profileAppService = GetRequiredService<IMyProfileAppService>();
            _fakeUserRepository = GetRequiredService<IIdentityUserRepository>();
        }

        [Fact]
        public async Task GetAsync_Should_Return_Current_User_Profile()
        {
            // 1. ARRANGE
            var userId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");

            var fakeUser = new IdentityUser(userId, "juanperez", "juan@goplaces.com");
            fakeUser.Name = "Juan";
            fakeUser.SetProperty("PhotoUrl", "https://foto.com/yo.jpg");
            fakeUser.SetProperty("Preferences", "Me gusta la playa");

            // 👇 Configuración infalible: Simulamos la búsqueda en BD
            _fakeUserRepository.FindAsync(userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(fakeUser));

            // 2. ACT
            var result = await _profileAppService.GetAsync();

            // 3. ASSERT
            result.ShouldNotBeNull();
            result.UserName.ShouldBe("juanperez");
            result.PhotoUrl.ShouldBe("https://foto.com/yo.jpg");
        }

        [Fact]
        public async Task UpdateAsync_Should_Save_Changes_To_Repository()
        {
            // 1. ARRANGE
            var userId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");
            var fakeUser = new IdentityUser(userId, "juanperez", "juan@goplaces.com");

            // Configuramos la búsqueda para que encuentre al usuario a editar
            _fakeUserRepository.FindAsync(userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(fakeUser));

            var input = new UserProfileDto
            {
                UserName = "juanperez",
                Email = "nuevo@email.com",
                Name = "Juan Actualizado",
                Surname = "Perez",
                PhotoUrl = "https://nueva-foto.com/img.png",
                Preferences = "Ahora prefiero montaña"
            };

            // 2. ACT
            await _profileAppService.UpdateAsync(input);

            // 3. ASSERT
            // Verificamos que se llamó al Update del Repositorio
            await _fakeUserRepository.Received().UpdateAsync(Arg.Any<IdentityUser>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());

            // Verificamos que los datos cambiaron en el objeto
            fakeUser.Name.ShouldBe("Juan Actualizado");
            fakeUser.GetProperty<string>("PhotoUrl").ShouldBe("https://nueva-foto.com/img.png");
        }
    }
}