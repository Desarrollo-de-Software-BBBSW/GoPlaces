using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Volo.Abp.Authorization;
using GoPlaces.Destinations;
using GoPlaces.Cities;
using Volo.Abp.Identity;

namespace GoPlaces.Ratings
{
    [Authorize]
    public class RatingAppService : ApplicationService, IRatingAppService
    {
        private readonly IRepository<Rating, Guid> _repo;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly ICitySearchService _citySearchService;
        private readonly IRepository<IdentityUser, Guid> _userRepository;

        public RatingAppService(
            IRepository<Rating, Guid> repo,
            IRepository<Destination, Guid> destinationRepository,
            ICitySearchService citySearchService,
            IRepository<IdentityUser, Guid> userRepository)
        {
            _repo = repo;
            _destinationRepository = destinationRepository;
            _citySearchService = citySearchService;
            _userRepository = userRepository;
        }

        public async Task<RatingDto> CreateAsync(CreateRatingDto input)
        {
            if (input.Score < 1 || input.Score > 5)
                throw new BusinessException("Rating.ScoreOutOfRange");

            var userId = CurrentUser.Id.Value;

            var destination = await _destinationRepository.FindAsync(input.DestinationId);

            if (destination == null)
            {
                var externalCity = await _citySearchService.GetByIdAsync(input.DestinationId);
                if (externalCity != null)
                {
                    destination = new Destination(
                        externalCity.Id,
                        externalCity.Name,
                        externalCity.Country,
                        0,
                        new Coordinates(0, 0)
                    );

                    await _destinationRepository.InsertAsync(destination, autoSave: true);
                }
                else
                {
                    throw new UserFriendlyException("No se pudo obtener la info de la ciudad.");
                }
            }

            var exists = await (await _repo.GetQueryableAsync())
                .AnyAsync(r => r.DestinationId == input.DestinationId && r.UserId == userId);

            if (exists) throw new UserFriendlyException("Ya has calificado este lugar.");

            var rating = new Rating(GuidGenerator.Create(), input.DestinationId, input.Score, input.Comment, userId);
            await _repo.InsertAsync(rating, autoSave: true);

            return ObjectMapper.Map<Rating, RatingDto>(rating);
        }

        public async Task<ListResultDto<RatingDto>> GetByDestinationAsync(Guid destinationId)
        {
            // 1. Buscamos primero las calificaciones del destino
            var ratings = await (await _repo.GetQueryableAsync())
                .Where(r => r.DestinationId == destinationId)
                .OrderByDescending(r => r.CreationTime)
                .ToListAsync();

            // Si no hay calificaciones, retornamos vacío de inmediato
            if (!ratings.Any())
            {
                return new ListResultDto<RatingDto>(new List<RatingDto>());
            }

            // 2. Extraemos los IDs de los usuarios que comentaron (sin repetir)
            var userIds = ratings.Select(r => r.UserId).Distinct().ToList();

            // 3. Buscamos los nombres de esos usuarios
            var users = await (await _userRepository.GetQueryableAsync())
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            // Creamos un diccionario para buscar los nombres súper rápido
            var userDictionary = users.ToDictionary(u => u.Id, u => u.UserName);

            // 4. Armamos la respuesta final cruzando los datos en memoria
            var dtos = new List<RatingDto>();
            foreach (var rating in ratings)
            {
                var dto = ObjectMapper.Map<Rating, RatingDto>(rating);
                // Si encontramos al usuario, le ponemos el nombre, si no, lo marcamos como Anónimo
                dto.UserName = userDictionary.ContainsKey(rating.UserId) ? userDictionary[rating.UserId] : "Anónimo";
                dtos.Add(dto);
            }

            return new ListResultDto<RatingDto>(dtos);
        }

        public async Task<RatingDto?> GetMyForDestinationAsync(Guid destinationId)
        {
            if (CurrentUser.Id == null) return null;
            var entity = await (await _repo.GetQueryableAsync())
                .Where(r => r.DestinationId == destinationId && r.UserId == CurrentUser.Id.Value)
                .SingleOrDefaultAsync();
            return entity == null ? null : ObjectMapper.Map<Rating, RatingDto>(entity);
        }

        public async Task<double> GetAverageRatingAsync(Guid destinationId)
        {
            var query = await _repo.GetQueryableAsync();
            var ratingsForDestination = query.Where(r => r.DestinationId == destinationId);

            // Si nadie ha calificado aún, devolvemos 0 para evitar errores en EF Core
            if (!await ratingsForDestination.AnyAsync())
            {
                return 0.0;
            }

            var average = await ratingsForDestination.AverageAsync(r => r.Score);

            // Redondeamos a 1 decimal (ej: 4.5) para que sea amigable en el frontend
            return Math.Round(average, 1);
        }

        // ✅ Lógica de Edición Protegida (Ahora usando rating.Update)
        public async Task<RatingDto> UpdateAsync(Guid id, CreateRatingDto input)
        {
            var rating = await _repo.GetAsync(id);

            // Verificación de seguridad: ¿Es el dueño?
            if (rating.UserId != CurrentUser.Id)
            {
                throw new AbpAuthorizationException("No tienes permiso para editar esta calificación.");
            }

            if (input.Score < 1 || input.Score > 5)
                throw new BusinessException("Rating.ScoreOutOfRange");

            // 👇 USAMOS EL NUEVO MÉTODO DE LA ENTIDAD EN LUGAR DE ASIGNAR DIRECTO
            rating.Update(input.Score, input.Comment);

            await _repo.UpdateAsync(rating, autoSave: true);

            return ObjectMapper.Map<Rating, RatingDto>(rating);
        }

        // ✅ Lógica de Eliminación Protegida
        public async Task DeleteAsync(Guid id)
        {
            var rating = await _repo.GetAsync(id);

            // Verificación de seguridad: ¿Es el dueño?
            if (rating.UserId != CurrentUser.Id)
            {
                throw new AbpAuthorizationException("No tienes permiso para eliminar esta calificación.");
            }

            await _repo.DeleteAsync(id, autoSave: true);
        }
    }
}