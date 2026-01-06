using System.Threading.Tasks;
using Xunit;
using GoPlaces.Users;
using Shouldly;
using Volo.Abp.Validation; // SOLUCIONA ERROR CS0246

namespace GoPlaces.Tests.Users
{
    // SOLUCIONA ERROR CS0305 y CS0103:
    // Agregamos <GoPlacesApplicationTestModule> para que sepa cómo arrancar la app
    public class RegisterAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly IMyRegisterAppService _registerAppService;

        public RegisterAppService_Tests()
        {
            // Ahora sí reconocerá este método
            _registerAppService = GetRequiredService<IMyRegisterAppService>();
        }

        [Fact]
        public async Task Should_Register_A_Valid_User()
        {
            // Arrange
            var input = new RegisterInputDto
            {
                UserName = "TestUserGoPlaces",
                Email = "test@goplaces.com",
                Password = "1Q2w3e4r5t6y()"
            };

            // Act
            // SOLUCIONA ERROR CS0815: Quitamos "var result ="
            await _registerAppService.RegisterAsync(input);

            // Assert
            // Si no hay excepción, la prueba pasa.
        }

        [Fact]
        public async Task Should_Fail_If_Email_Is_Invalid()
        {
            var input = new RegisterInputDto
            {
                UserName = "UserInvalid",
                Email = "esto-no-es-un-email",
                Password = "Password123!"
            };

            await Assert.ThrowsAsync<AbpValidationException>(async () =>
            {
                await _registerAppService.RegisterAsync(input);
            });
        }
    }
}