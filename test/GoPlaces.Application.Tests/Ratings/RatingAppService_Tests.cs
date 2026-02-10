using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Volo.Abp;

namespace GoPlaces.Ratings;

public class RatingAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
{
    private readonly IRatingAppService _ratingService;

    public RatingAppService_Tests()
    {
        _ratingService = GetRequiredService<IRatingAppService>();
    }

    [Fact]
    public async Task Should_Create_Rating()
    {
        // ARRANGE
        var destinationId = 33924; // Paris
        var input = new CreateRatingDto
        {
            DestinationId = destinationId,
            Score = 5,
            Comment = "¡Excelente lugar!"
        };

        // ACT
        var result = await _ratingService.CreateAsync(input);

        // ASSERT
        result.ShouldNotBeNull();
        result.Score.ShouldBe(5);
        result.DestinationId.ShouldBe(destinationId);
    }

    [Fact]
    public async Task Should_Not_Allow_Duplicate_Rating()
    {
        // ARRANGE
        var destinationId = 12345;
        var input = new CreateRatingDto { DestinationId = destinationId, Score = 4 };

        // Creamos la primera vez
        await _ratingService.CreateAsync(input);

        // ACT & ASSERT
        // Intentamos crear la segunda vez para el mismo destino
        await Assert.ThrowsAsync<UserFriendlyException>(async () =>
        {
            await _ratingService.CreateAsync(input);
        });
    }
}