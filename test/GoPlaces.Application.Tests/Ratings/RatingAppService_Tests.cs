using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GoPlaces.Cities;
using GoPlaces.Ratings;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Guids;
using Volo.Abp.Security.Claims;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Authorization; // ✅ Necesario para AbpAuthorizationException
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

        // Método auxiliar para cambiar el usuario en contexto más limpio
        private IDisposable ChangeUserContext(Guid userId, string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(AbpClaimTypes.UserId, userId.ToString()),
                new Claim(AbpClaimTypes.UserName, userName)
            };
            return _currentPrincipalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));
        }

        // Método auxiliar para crear el servicio con el Dependency Injection correcto
        private RatingAppService CreateRatingService()
        {
            var service = new RatingAppService(_ratingRepo, _destinationRepo, _cityService);
            service.LazyServiceProvider = ServiceProvider.GetRequiredService<Volo.Abp.DependencyInjection.IAbpLazyServiceProvider>();
            return service;
        }

        [Fact]
        public async Task Should_Create_Rating()
        {
            var destinationId = _guidGenerator.Create();

            var destination = new DestinationEntity(
                destinationId, "Paris", "France", 2000000, new CoordinatesValue(48.85, 2.35), "paris.jpg", DateTime.UtcNow);

            await _destinationRepo.InsertAsync(destination);

            var userId = _guidGenerator.Create();

            using (ChangeUserContext(userId, "testuser"))
            {
                var ratingService = CreateRatingService();

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

        [Fact]
        public async Task Should_Throw_Exception_When_User_Rates_Twice()
        {
            var destinationId = _guidGenerator.Create();
            var destination = new DestinationEntity(
                destinationId, "Rome", "Italy", 1000, new CoordinatesValue(0, 0), "img.jpg", DateTime.UtcNow);
            await _destinationRepo.InsertAsync(destination);

            var userId = _guidGenerator.Create();

            using (ChangeUserContext(userId, "duplicateuser"))
            {
                var ratingService = CreateRatingService();

                var input = new CreateRatingDto { DestinationId = destinationId, Score = 4, Comment = "First" };

                // Primera votación: Éxito
                await ratingService.CreateAsync(input);

                // Segunda votación: Debe fallar
                await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await ratingService.CreateAsync(input);
                });
            }
        }

        // 👇👇👇 NUEVAS PRUEBAS DE EDICIÓN Y ELIMINACIÓN 👇👇👇

        [Fact]
        public async Task Should_Update_Rating_If_Owner()
        {
            var destinationId = _guidGenerator.Create();
            await _destinationRepo.InsertAsync(new DestinationEntity(destinationId, "London", "UK", 1000, new CoordinatesValue(0, 0), "img.jpg", DateTime.UtcNow));

            var ownerId = _guidGenerator.Create();
            Guid ratingId = Guid.Empty;

            // 1. El dueño CREA la calificación (Aislado en UnitOfWork)
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(ownerId, "owneruser"))
                {
                    var ratingService = CreateRatingService();
                    var created = await ratingService.CreateAsync(new CreateRatingDto { DestinationId = destinationId, Score = 3, Comment = "Normal" });
                    ratingId = created.Id;
                }
            });

            // 2. El dueño EDITA la calificación
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(ownerId, "owneruser"))
                {
                    var ratingService = CreateRatingService();
                    var result = await ratingService.UpdateAsync(ratingId, new CreateRatingDto { DestinationId = destinationId, Score = 5, Comment = "Mucho mejor ahora" });

                    result.Score.ShouldBe(5);
                    result.Comment.ShouldBe("Mucho mejor ahora");
                }
            });
        }

        [Fact]
        public async Task Should_Fail_Update_If_Not_Owner()
        {
            var destinationId = _guidGenerator.Create();
            await _destinationRepo.InsertAsync(new DestinationEntity(destinationId, "Berlin", "Germany", 1000, new CoordinatesValue(0, 0), "img.jpg", DateTime.UtcNow));

            var ownerId = _guidGenerator.Create();
            var hackerId = _guidGenerator.Create();
            Guid ratingId = Guid.Empty;

            // 1. El dueño CREA la calificación
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(ownerId, "owneruser"))
                {
                    var ratingService = CreateRatingService();
                    var created = await ratingService.CreateAsync(new CreateRatingDto { DestinationId = destinationId, Score = 5, Comment = "Perfecto" });
                    ratingId = created.Id;
                }
            });

            // 2. El HACKER intenta EDITARLA
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(hackerId, "hackeruser"))
                {
                    var ratingService = CreateRatingService();

                    await Assert.ThrowsAsync<AbpAuthorizationException>(async () =>
                    {
                        await ratingService.UpdateAsync(ratingId, new CreateRatingDto { DestinationId = destinationId, Score = 1, Comment = "Hackeado" });
                    });
                }
            });
        }

        [Fact]
        public async Task Should_Delete_Rating_If_Owner()
        {
            var destinationId = _guidGenerator.Create();
            await _destinationRepo.InsertAsync(new DestinationEntity(destinationId, "Madrid", "Spain", 1000, new CoordinatesValue(0, 0), "img.jpg", DateTime.UtcNow));

            var ownerId = _guidGenerator.Create();
            Guid ratingId = Guid.Empty;

            // 1. El dueño CREA la calificación
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(ownerId, "owneruser"))
                {
                    var ratingService = CreateRatingService();
                    var created = await ratingService.CreateAsync(new CreateRatingDto { DestinationId = destinationId, Score = 4, Comment = "Aceptable" });
                    ratingId = created.Id;
                }
            });

            // 2. El dueño ELIMINA la calificación
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(ownerId, "owneruser"))
                {
                    var ratingService = CreateRatingService();
                    await ratingService.DeleteAsync(ratingId);

                    // Verificamos que ya no existe (GetMyForDestination debería retornar null)
                    var afterDelete = await ratingService.GetMyForDestinationAsync(destinationId);
                    afterDelete.ShouldBeNull();
                }
            });
        }

        [Fact]
        public async Task Should_Fail_Delete_If_Not_Owner()
        {
            var destinationId = _guidGenerator.Create();
            await _destinationRepo.InsertAsync(new DestinationEntity(destinationId, "Lisbon", "Portugal", 1000, new CoordinatesValue(0, 0), "img.jpg", DateTime.UtcNow));

            var ownerId = _guidGenerator.Create();
            var hackerId = _guidGenerator.Create();
            Guid ratingId = Guid.Empty;

            // 1. El dueño CREA la calificación
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(ownerId, "owneruser"))
                {
                    var ratingService = CreateRatingService();
                    var created = await ratingService.CreateAsync(new CreateRatingDto { DestinationId = destinationId, Score = 5, Comment = "Increíble" });
                    ratingId = created.Id;
                }
            });

            // 2. El HACKER intenta ELIMINARLA
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(hackerId, "hackeruser"))
                {
                    var ratingService = CreateRatingService();

                    await Assert.ThrowsAsync<AbpAuthorizationException>(async () =>
                    {
                        await ratingService.DeleteAsync(ratingId);
                    });
                }
            });
        }
    }
}