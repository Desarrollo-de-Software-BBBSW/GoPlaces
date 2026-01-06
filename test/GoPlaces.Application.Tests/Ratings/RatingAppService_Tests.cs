using GoPlaces.Ratings;
using Shouldly;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;
using Xunit;

namespace GoPlaces.Tests.Ratings;

public class RatingAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
{
    private readonly IRatingAppService _ratingAppService;

    public RatingAppService_Tests()
    {
        _ratingAppService = GetRequiredService<IRatingAppService>();
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Rating_With_Normalized_Comment()
    {
        var destinationId = Guid.NewGuid();

        var input = new CreateRatingDto
        {
            DestinationId = destinationId,
            Score = 4,
            Comment = "  Hermoso lugar  "
        };

        var result = await _ratingAppService.CreateAsync(input);

        result.DestinationId.ShouldBe(destinationId);
        result.Score.ShouldBe(4);
        result.Comment.ShouldBe("Hermoso lugar");
    }

    [Fact]
    public async Task CreateAsync_Should_Set_Null_For_Empty_Comment()
    {
        var destinationId = Guid.NewGuid();

        var input = new CreateRatingDto
        {
            DestinationId = destinationId,
            Score = 5,
            Comment = "   "
        };

        var result = await _ratingAppService.CreateAsync(input);

        result.Comment.ShouldBeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Score_Is_Out_Of_Range()
    {
        var invalidScoreInput = new CreateRatingDto
        {
            DestinationId = Guid.NewGuid(),
            Score = 6
        };

        await Should.ThrowAsync<AbpValidationException>(() =>
            _ratingAppService.CreateAsync(invalidScoreInput));
    }

    [Fact]
    public async Task CreateAsync_Should_Prevent_Duplicate_Ratings_For_User_And_Destination()
    {
        var destinationId = Guid.NewGuid();

        var firstInput = new CreateRatingDto
        {
            DestinationId = destinationId,
            Score = 5,
            Comment = "Primera"
        };

        await _ratingAppService.CreateAsync(firstInput);

        var duplicateInput = new CreateRatingDto
        {
            DestinationId = destinationId,
            Score = 3,
            Comment = "Segunda"
        };

        await Should.ThrowAsync<BusinessException>(() =>
            _ratingAppService.CreateAsync(duplicateInput));
    }
    [Fact]
    public async Task CreateAsync_Should_Throw_AuthorizationException_When_User_Is_Not_LoggedIn()
    {
        // 1. "Cerramos sesión" simuladamente
        FakeCurrentPrincipalAccessor.IsAuthenticated = false;

        var input = new CreateRatingDto
        {
            DestinationId = Guid.NewGuid(),
            Score = 5,
            Comment = "Intento de hackeo"
        };

        try
        {
            // 2. Esperamos que ABP lance excepción de Autorización (401/403)
            await Should.ThrowAsync<Volo.Abp.Authorization.AbpAuthorizationException>(() =>
                _ratingAppService.CreateAsync(input));
        }
        finally
        {
            // 3. IMPORTANTE: Volvemos a activar el usuario para no romper otros tests
            FakeCurrentPrincipalAccessor.IsAuthenticated = true;
        }
    }
    [Fact]
    public async Task CreateAsync_Should_Associate_Current_User_To_Rating()
    {
        // El ID que pusimos fijo en el FakeCurrentPrincipalAccessor
        var currentUserId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");

        var input = new CreateRatingDto
        {
            DestinationId = Guid.NewGuid(),
            Score = 5,
            Comment = "Probando UserID"
        };

        // Act
        var result = await _ratingAppService.CreateAsync(input);

        // Assert
        // Verificamos que el sistema (IUserOwned) haya guardado el ID del creador
        result.UserId.ShouldBe(currentUserId);
    }
}
