using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Xunit;

// 👇 Alias explícitos para evitar conflictos de nombres
using DestinationEntity = GoPlaces.Destinations.Destination;
using CoordinatesValue = GoPlaces.Destinations.Coordinates;
using GoPlaces.Destinations;

namespace GoPlaces.Experiences
{
    public class ExperienceAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly IExperienceAppService _experienceAppService;
        private readonly IRepository<DestinationEntity, Guid> _destinationRepository;
        private readonly IGuidGenerator _guidGenerator;

        public ExperienceAppService_Tests()
        {
            _experienceAppService = GetRequiredService<IExperienceAppService>();
            _destinationRepository = GetRequiredService<IRepository<DestinationEntity, Guid>>();
            _guidGenerator = GetRequiredService<IGuidGenerator>();
        }

        [Fact]
        public async Task Should_Create_Experience_When_Destination_Exists()
        {
            // ARRANGE
            var destinationId = _guidGenerator.Create();

            var destination = new DestinationEntity(
                destinationId,
                "Tokyo",
                "Japan",
                14000000,
                new CoordinatesValue(35.67, 139.65),
                "tokyo.jpg"
            );
            await _destinationRepository.InsertAsync(destination);

            var input = new CreateUpdateExperienceDto
            {
                DestinationId = destinationId,
                Title = "Tour de Sushi",
                Description = "El mejor sushi de la ciudad",
                Price = 150.00m,
                Date = DateTime.Now.AddDays(5)
            };

            // ACT
            var result = await _experienceAppService.CreateAsync(input);

            // ASSERT
            result.ShouldNotBeNull();
            result.Id.ShouldNotBe(Guid.Empty);
            result.Title.ShouldBe("Tour de Sushi");
            result.DestinationId.ShouldBe(destinationId);
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Destination_Does_Not_Exist()
        {
            // ARRANGE
            var fakeDestinationId = Guid.NewGuid();

            var input = new CreateUpdateExperienceDto
            {
                DestinationId = fakeDestinationId,
                Title = "Experiencia Fantasma",
                Description = "Descripción de prueba", // ✅ Agregado para evitar error de validación antes del error de negocio
                Price = 100,
                Date = DateTime.Now
            };

            // ACT & ASSERT
            await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            {
                await _experienceAppService.CreateAsync(input);
            });
        }

        [Fact]
        public async Task Should_Get_List_Of_Experiences()
        {
            // ARRANGE
            var destId = _guidGenerator.Create();

            await _destinationRepository.InsertAsync(new DestinationEntity(destId, "Rome", "Italy", 2000, new CoordinatesValue(0, 0), "rome.jpg"));

            var input = new CreateUpdateExperienceDto
            {
                DestinationId = destId,
                Title = "Visita al Coliseo",
                Description = "Una visita guiada por el antiguo anfiteatro.", // ✅ CORRECCIÓN PRINCIPAL: Agregamos Description
                Price = 50,
                Date = DateTime.Now
            };
            await _experienceAppService.CreateAsync(input);

            // ACT
            var result = await _experienceAppService.GetListAsync(new Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto());

            // ASSERT
            result.TotalCount.ShouldBeGreaterThan(0);
            result.Items.ShouldContain(x => x.Title == "Visita al Coliseo");
        }
    }
}