using GoPlaces.Destinations;
using GoPlaces.EntityFrameworkCore;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Volo.Abp.Validation;
using Xunit;

namespace GoPlaces.Destination
{
    public abstract class DestinationAppService_Tests<TStartupModule> : GoPlacesApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly IDestinationAppService _service;
        private readonly IDbContextProvider<GoPlacesDbContext> _dbContextProvider;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        protected DestinationAppService_Tests()
        {
            _service = GetRequiredService<IDestinationAppService>();
            _dbContextProvider = GetRequiredService<IDbContextProvider<GoPlacesDbContext>>();
            _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
        }
        [Fact]
        public async Task CreateAsync_ShouldReturnCreatedDestinationDto()
        {
            var input = new CreateUpdateDestinationDto
            {
                Name = "Paris",
                Country = "France",
                Population = 1000000,
                Latitude = 12.34,
                Longitude = 56.78,
                ImageUrl = "http://example.com/image.jpg"
            };

            //Actividad a realizar
            var result = await _service.CreateAsync(input);

            //Assert (Validamos el resultado del test)
            result.ShouldNotBeNull();
            result.Id.ShouldNotBe(Guid.Empty);
            result.Name.ShouldBe(input.Name);
            result.Country.ShouldBe(input.Country);
            result.Population.ShouldBe(input.Population);
            result.Latitude.ShouldBe(input.Latitude);
            result.Longitude.ShouldBe(input.Longitude);
            result.ImageUrl.ShouldBe(input.ImageUrl);
        }

        [Fact]
        public async Task CreateAsync_ShouldPersistDestinationInDatabase()
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                //Arrange
                var input = new CreateUpdateDestinationDto
                {
                    Name = "Tokyo",
                    Country = "Japan",
                    Population = 14000000,
                    Latitude = 35.68,
                    Longitude = 139.76,
                    ImageUrl = "http://example.com/tokyo.jpg"
                };
                //Actividad a realizar
                var result = await _service.CreateAsync(input);

                var dbContext = await _dbContextProvider.GetDbContextAsync();
                var destinationInDb = await dbContext.Destinations.FindAsync(result.Id);

                //Assert (Validamos el resultado del test)
                destinationInDb.ShouldNotBeNull();
                destinationInDb.Name.ShouldBe(input.Name);
                destinationInDb.Country.ShouldBe(input.Country);
                destinationInDb.Population.ShouldBe(input.Population);
                destinationInDb.Coordinates.Latitude.ShouldBe(input.Latitude);
                destinationInDb.Coordinates.Longitude.ShouldBe(input.Longitude);
                destinationInDb.ImageUrl.ShouldBe(input.ImageUrl);
            }

        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenCountryIsNull()
        {
            var input = new CreateUpdateDestinationDto
            {
                Name = "NeW York", 
                Country = "", // Country vacío
                Population = 500000,
                Latitude = 40.71,
                Longitude = -74.01,
                ImageUrl = "http://example.com/nyc.jpg"
            };
            await Should.ThrowAsync<AbpValidationException>(async () =>
            {
                await _service.CreateAsync(input);
            });
        }
    }
}
