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
using GoPlaces.Destinations; // 👈 Necesario para Coordinates
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

            // 1. Verificamos si ya existe el destino localmente
            var destination = await _destinationRepository.FindAsync(input.DestinationId);

            if (destination == null)
            {
                // 2. Si no existe, lo traemos de GeoDB e insertamos
                var externalCity = await _citySearchService.GetByIdAsync(input.DestinationId);
                if (externalCity != null)
                {
                    // ✅ CONSTRUCTOR CORREGIDO: Usamos population: 0 y Coordinates(0,0)
                    destination = new Destination(
                        externalCity.Id,
                        externalCity.Name,
                        externalCity.Country,
                        0,                           // population
                        new Coordinates(0, 0)        // coordinates
                    );

                    await _destinationRepository.InsertAsync(destination, autoSave: true);
                }
                else
                {
                    throw new UserFriendlyException("No se pudo obtener la info de la ciudad.");
                }
            }

            // 3. Verificamos si ya calificó
            var exists = await (await _repo.GetQueryableAsync())
                .AnyAsync(r => r.DestinationId == input.DestinationId && r.UserId == userId);

            if (exists) throw new UserFriendlyException("Ya has calificado este lugar.");

            // 4. Guardamos Rating
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
    }
}