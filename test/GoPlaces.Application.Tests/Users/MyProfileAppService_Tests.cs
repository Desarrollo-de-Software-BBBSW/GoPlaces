using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using NSubstitute;
using Volo.Abp.Identity;
using GoPlaces.Users;
using Volo.Abp.Data;
using Microsoft.AspNetCore.Identity; // Para IPasswordHasher

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
        [Fact]
        public async Task ChangePasswordAsync_Should_Work_When_CurrentPassword_Is_Correct()
        {
            // 1. ARRANGE
            var userId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");
            var currentPassword = "Password123!";
            var newPassword = "NewPassword123!";

            var fakeUser = new IdentityUser(userId, "juanperez", "juan@goplaces.com");

            // TRUCO CLAVE: Como IdentityUserManager es real, verificará el hash.
            // Necesitamos inyectar el Hasher real para generarle un hash válido al usuario falso.
            var passwordHasher = GetRequiredService<IPasswordHasher<IdentityUser>>();
            typeof(IdentityUser).GetProperty("PasswordHash")
            .SetValue(fakeUser, passwordHasher.HashPassword(fakeUser, currentPassword));

            // Simulamos que la BD encuentra al usuario
            _fakeUserRepository.FindAsync(userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(fakeUser));

            var input = new ChangePasswordInputDto
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            // 2. ACT
            await _profileAppService.ChangePasswordAsync(input);

            // 3. ASSERT
            // Verificamos que se llamó al UpdateAsync del repositorio (significa que IdentityUserManager intentó guardar)
            await _fakeUserRepository.Received(1).UpdateAsync(Arg.Is<IdentityUser>(u => u.Id == userId), Arg.Any<bool>(), Arg.Any<CancellationToken>());

            // Opcional: Verificar que el hash cambió (no será igual al original)
            fakeUser.PasswordHash.ShouldNotBe(passwordHasher.HashPassword(fakeUser, currentPassword));
        }

        [Fact]
        public async Task ChangePasswordAsync_Should_Throw_Exception_When_CurrentPassword_Is_Wrong()
        {
            // 1. ARRANGE
            var userId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");
            var realPassword = "Password123!";
            var wrongPassword = "WrongPassword!";

            var fakeUser = new IdentityUser(userId, "juanperez", "juan@goplaces.com");

            // Seteamos la contraseña real
            var passwordHasher = GetRequiredService<IPasswordHasher<IdentityUser>>();
            typeof(IdentityUser).GetProperty("PasswordHash")
            .SetValue(fakeUser, passwordHasher.HashPassword(fakeUser, realPassword));

            _fakeUserRepository.FindAsync(userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(fakeUser));

            var input = new ChangePasswordInputDto
            {
                CurrentPassword = wrongPassword, // <--- Enviamos la incorrecta
                NewPassword = "NewPassword123!"
            };

            // 2. ACT & ASSERT
            // Esperamos que ABP lance una excepción de validación (AbpValidationException o UserFriendlyException)
            // IdentityUserManager suele devolver errores que ABP convierte en excepciones.
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await _profileAppService.ChangePasswordAsync(input);
            });

            // Aseguramos que NO se llamó a UpdateAsync porque falló la validación
            await _fakeUserRepository.DidNotReceive().UpdateAsync(Arg.Any<IdentityUser>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }
    }
}