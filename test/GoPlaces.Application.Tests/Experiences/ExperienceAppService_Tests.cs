using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Security.Claims;
using Xunit;

// Alias para evitar conflictos
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
        private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

        public ExperienceAppService_Tests()
        {
            _experienceAppService = GetRequiredService<IExperienceAppService>();
            _destinationRepository = GetRequiredService<IRepository<DestinationEntity, Guid>>();
            _guidGenerator = GetRequiredService<IGuidGenerator>();
            _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
        }

        private IDisposable CambiarUsuario(Guid userId)
        {
            var claims = new List<Claim> { new Claim(AbpClaimTypes.UserId, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            return _currentPrincipalAccessor.Change(principal);
        }

        [Fact]
        public async Task Should_Create_Experience_When_Destination_Exists()
        {
            var destinationId = _guidGenerator.Create();
            await _destinationRepository.InsertAsync(new DestinationEntity(destinationId, "Tokyo", "Japan", 14000000, new CoordinatesValue(35.67, 139.65), "tokyo.jpg"));

            var input = new CreateUpdateExperienceDto
            {
                DestinationId = destinationId,
                Title = "Tour de Sushi",
                Description = "El mejor sushi",
                Price = 150.00m,
                Date = DateTime.Now.AddDays(5),
                Rating = "Positiva"
            };

            var result = await _experienceAppService.CreateAsync(input);

            result.ShouldNotBeNull();
            result.Title.ShouldBe("Tour de Sushi");
            result.Rating.ShouldBe("Positiva");
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Destination_Does_Not_Exist()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var fakeDestinationId = Guid.NewGuid();
                var input = new CreateUpdateExperienceDto
                {
                    DestinationId = fakeDestinationId,
                    Title = "Experiencia Fantasma",
                    Description = "Test",
                    Price = 100,
                    Date = DateTime.Now,
                    Rating = "Positiva"
                };

                await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _experienceAppService.CreateAsync(input);
                });
            });
        }

        [Fact]
        public async Task Should_Get_List_Of_Experiences()
        {
            var destId = _guidGenerator.Create();
            await _destinationRepository.InsertAsync(new DestinationEntity(destId, "Rome", "Italy", 2000, new CoordinatesValue(0, 0), "rome.jpg"));

            var input = new CreateUpdateExperienceDto
            {
                DestinationId = destId,
                Title = "Visita al Coliseo",
                Description = "Una visita guiada",
                Price = 50,
                Date = DateTime.Now,
                Rating = "Positiva"
            };
            await _experienceAppService.CreateAsync(input);

            var result = await _experienceAppService.GetListAsync(new Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto());

            result.TotalCount.ShouldBeGreaterThan(0);
            result.Items.ShouldContain(x => x.Title == "Visita al Coliseo");
        }

        // 👇 CORREGIDO: UnitOfWork separado para evitar choques en SQLite
        [Fact]
        public async Task Should_Update_Experience_If_Owner()
        {
            var ownerId = Guid.NewGuid();
            var destId = _guidGenerator.Create();
            await _destinationRepository.InsertAsync(new DestinationEntity(destId, "Paris", "France", 2000, new CoordinatesValue(0, 0), "img.jpg"));

            Guid experienceId = Guid.Empty;

            await WithUnitOfWorkAsync(async () =>
            {
                using (CambiarUsuario(ownerId))
                {
                    var createInput = new CreateUpdateExperienceDto
                    {
                        DestinationId = destId,
                        Title = "Titulo Original",
                        Description = "Original",
                        Price = 10,
                        Date = DateTime.Now,
                        Rating = "Positiva"
                    };
                    var created = await _experienceAppService.CreateAsync(createInput);
                    experienceId = created.Id;
                }
            });

            await WithUnitOfWorkAsync(async () =>
            {
                using (CambiarUsuario(ownerId))
                {
                    var updateInput = new CreateUpdateExperienceDto
                    {
                        DestinationId = destId,
                        Title = "Titulo EDITADO",
                        Description = "Original",
                        Price = 20,
                        Date = DateTime.Now,
                        Rating = "Neutra"
                    };

                    var result = await _experienceAppService.UpdateAsync(experienceId, updateInput);
                    result.Title.ShouldBe("Titulo EDITADO");
                    result.Rating.ShouldBe("Neutra");
                }
            });
        }

        [Fact]
        public async Task Should_Fail_Update_If_Not_Owner()
        {
            var ownerId = Guid.NewGuid();
            var hackerId = Guid.NewGuid();
            var destId = _guidGenerator.Create();
            await _destinationRepository.InsertAsync(new DestinationEntity(destId, "London", "UK", 2000, new CoordinatesValue(0, 0), "img.jpg"));

            Guid experienceId;

            using (CambiarUsuario(ownerId))
            {
                var created = await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto
                {
                    DestinationId = destId,
                    Title = "My Precious",
                    Description = "Don't touch",
                    Price = 100,
                    Date = DateTime.Now,
                    Rating = "Positiva"
                });
                experienceId = created.Id;
            }

            using (CambiarUsuario(hackerId))
            {
                var updateInput = new CreateUpdateExperienceDto
                {
                    DestinationId = destId,
                    Title = "HACKED!",
                    Description = "Stolen",
                    Price = 0,
                    Date = DateTime.Now,
                    Rating = "Negativa"
                };

                await WithUnitOfWorkAsync(async () =>
                {
                    await Assert.ThrowsAsync<AbpAuthorizationException>(async () =>
                    {
                        await _experienceAppService.UpdateAsync(experienceId, updateInput);
                    });
                });
            }
        }

        [Fact]
        public async Task Should_Delete_Experience_If_Owner()
        {
            var ownerId = Guid.NewGuid();
            var destId = _guidGenerator.Create();
            await _destinationRepository.InsertAsync(new DestinationEntity(destId, "Berlin", "Germany", 3000, new CoordinatesValue(0, 0), "img.jpg"));

            Guid experienceId;

            using (CambiarUsuario(ownerId))
            {
                var created = await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto
                {
                    DestinationId = destId,
                    Title = "Para Borrar",
                    Description = "Se va a eliminar",
                    Price = 10,
                    Date = DateTime.Now,
                    Rating = "Positiva"
                });
                experienceId = created.Id;
            }

            using (CambiarUsuario(ownerId))
            {
                await _experienceAppService.DeleteAsync(experienceId);

                var result = await _experienceAppService.GetListAsync(new Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto());
                result.Items.ShouldNotContain(x => x.Id == experienceId);
            }
        }

        [Fact]
        public async Task Should_Fail_Delete_If_Not_Owner()
        {
            var ownerId = Guid.NewGuid();
            var hackerId = Guid.NewGuid();
            var destId = _guidGenerator.Create();
            await _destinationRepository.InsertAsync(new DestinationEntity(destId, "Madrid", "Spain", 3000, new CoordinatesValue(0, 0), "img.jpg"));

            Guid experienceId;

            using (CambiarUsuario(ownerId))
            {
                var created = await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto
                {
                    DestinationId = destId,
                    Title = "No me borres",
                    Description = "Seguro",
                    Price = 100,
                    Date = DateTime.Now,
                    Rating = "Positiva"
                });
                experienceId = created.Id;
            }

            using (CambiarUsuario(hackerId))
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    await Assert.ThrowsAsync<AbpAuthorizationException>(async () =>
                    {
                        await _experienceAppService.DeleteAsync(experienceId);
                    });
                });
            }
        }

        [Fact]
        public async Task Should_Get_Other_Users_Experiences()
        {
            var miUsuarioId = Guid.NewGuid();
            var otroUsuarioId = Guid.NewGuid();
            var destId = _guidGenerator.Create();

            await _destinationRepository.InsertAsync(new DestinationEntity(destId, "Lisbon", "Portugal", 5000, new CoordinatesValue(0, 0), "img.jpg"));

            using (CambiarUsuario(otroUsuarioId))
            {
                await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto
                {
                    DestinationId = destId,
                    Title = "Experiencia de un Desconocido",
                    Description = "No es mía",
                    Price = 50,
                    Date = DateTime.Now,
                    Rating = "Positiva"
                });
            }

            using (CambiarUsuario(miUsuarioId))
            {
                await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto
                {
                    DestinationId = destId,
                    Title = "Mi Experiencia Secreta",
                    Description = "Esta no debería salir",
                    Price = 20,
                    Date = DateTime.Now,
                    Rating = "Negativa"
                });
            }

            using (CambiarUsuario(miUsuarioId))
            {
                var result = await _experienceAppService.GetOtherUsersExperiencesAsync(destId);

                result.Items.ShouldContain(x => x.Title == "Experiencia de un Desconocido");
                result.Items.ShouldNotContain(x => x.Title == "Mi Experiencia Secreta");
            }
        }

        // 👇 CORREGIDO: Agregada la propiedad Description para cumplir el NOT NULL
        [Fact]
        public async Task Should_Filter_Experiences_By_Rating()
        {
            var destId = _guidGenerator.Create();
            await _destinationRepository.InsertAsync(new DestinationEntity(destId, "Amsterdam", "Netherlands", 1000, new CoordinatesValue(0, 0), "img.jpg"));

            // 1. Creamos 3 experiencias con distintas valoraciones
            await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto
            {
                DestinationId = destId,
                Title = "Excelente Tour",
                Description = "Descripción de prueba",
                Price = 10,
                Date = DateTime.Now,
                Rating = "Positiva"
            });
            await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto
            {
                DestinationId = destId,
                Title = "Mala comida",
                Description = "Descripción de prueba",
                Price = 10,
                Date = DateTime.Now,
                Rating = "Negativa"
            });
            await _experienceAppService.CreateAsync(new CreateUpdateExperienceDto
            {
                DestinationId = destId,
                Title = "Tour Normal",
                Description = "Descripción de prueba",
                Price = 10,
                Date = DateTime.Now,
                Rating = "Neutra"
            });

            // 2. Filtramos solo las positivas
            var result = await _experienceAppService.GetExperiencesByRatingAsync("Positiva");

            // 3. ASSERT
            result.Items.ShouldContain(x => x.Title == "Excelente Tour");
            result.Items.ShouldNotContain(x => x.Title == "Mala comida");
            result.Items.ShouldNotContain(x => x.Title == "Tour Normal");
        }
    }
}