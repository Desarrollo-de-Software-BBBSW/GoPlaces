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

namespace GoPlaces.Ratings
{
    [Authorize]
    public class RatingAppService : ApplicationService, IRatingAppService
    {
        private readonly IRepository<Rating, Guid> _repo;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly ICitySearchService _citySearchService;

        public RatingAppService(
            IRepository<Rating, Guid> repo,
            IRepository<Destination, Guid> destinationRepository,
            ICitySearchService citySearchService)
        {
            _repo = repo;
            _destinationRepository = destinationRepository;
            _citySearchService = citySearchService;
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
            var list = await (await _repo.GetQueryableAsync())
                .Where(r => r.DestinationId == destinationId)
                .OrderByDescending(r => r.CreationTime)
                .ToListAsync();

            return new ListResultDto<RatingDto>(
                ObjectMapper.Map<List<Rating>, List<RatingDto>>(list)
            );
        }

        public async Task<RatingDto?> GetMyForDestinationAsync(Guid destinationId)
        {
            if (CurrentUser.Id == null) return null;
            var entity = await (await _repo.GetQueryableAsync())
                .Where(r => r.DestinationId == destinationId && r.UserId == CurrentUser.Id.Value)
                .SingleOrDefaultAsync();
            return entity == null ? null : ObjectMapper.Map<Rating, RatingDto>(entity);
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