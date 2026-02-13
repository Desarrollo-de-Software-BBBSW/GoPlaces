using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GoPlaces.Cities;
using GoPlaces.Ratings;
using Shouldly;
using Volo.Abp.Guids;
using Volo.Abp.Security.Claims;
using Volo.Abp.Domain.Repositories;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

using DestinationEntity = GoPlaces.Destinations.Destination;
using CoordinatesValue = GoPlaces.Destinations.Coordinates;

namespace GoPlaces.Ratings
{
    public class RatingAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly IRepository<DestinationEntity, Guid> _destinationRepo;
        private readonly IRepository<Rating, Guid> _ratingRepo;
        private readonly ICitySearchService _cityService;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

        public RatingAppService_Tests()
        {
            _destinationRepo = GetRequiredService<IRepository<DestinationEntity, Guid>>();
            _ratingRepo = GetRequiredService<IRepository<Rating, Guid>>();
            _cityService = GetRequiredService<ICitySearchService>();
            _guidGenerator = GetRequiredService<IGuidGenerator>();
            _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
        }

        [Fact]
        public async Task Should_Create_Rating()
        {
            var destinationId = _guidGenerator.Create();

            var destination = new DestinationEntity(
                destinationId,
                "Paris",
                "France",
                2000000,
                new CoordinatesValue(48.85, 2.35),
                "paris.jpg",
                DateTime.UtcNow
            );

            await _destinationRepo.InsertAsync(destination);

            var userId = _guidGenerator.Create();
            var claims = new List<Claim>
            {
                new Claim(AbpClaimTypes.UserId, userId.ToString()),
                new Claim(AbpClaimTypes.UserName, "testuser")
            };

            using (_currentPrincipalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))))
            {
                var ratingService = new RatingAppService(_ratingRepo, _destinationRepo, _cityService);
                ratingService.LazyServiceProvider = ServiceProvider.GetRequiredService<Volo.Abp.DependencyInjection.IAbpLazyServiceProvider>();

                var input = new CreateRatingDto
                {
                    DestinationId = destinationId,
                    Score = 5,
                    Comment = "Perfect!"
                };

                var result = await ratingService.CreateAsync(input);

                result.ShouldNotBeNull();
                result.Score.ShouldBe(5);
            }
        }
    }
}