using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

namespace GoPlaces.Ratings;

[Authorize]
public class RatingAppService : ApplicationService, IRatingAppService
{
    private readonly IRepository<Rating, Guid> _repo;

    public RatingAppService(IRepository<Rating, Guid> repo)
    {
        _repo = repo;
    }

    public async Task<RatingDto> CreateAsync(CreateRatingDto input)
    {
        // Validación de rango (BusinessException está bien aquí, o UserFriendly si quieres mostrarlo en UI)
        if (input.Score < 1 || input.Score > 5)
            throw new BusinessException("Rating.ScoreOutOfRange").WithData("Score", input.Score);

        if (CurrentUser.Id == null)
        {
            throw new AbpAuthorizationException("Usuario no autenticado.");
        }
        var userId = CurrentUser.Id.Value;

        // Verificamos si ya existe
        var exists = await (await _repo.GetQueryableAsync())
            .AnyAsync(r => r.DestinationId == input.DestinationId && r.UserId == userId);

        if (exists)
        {
            // CORRECCIÓN PRINCIPAL PARA EL TEST:
            // Cambiamos BusinessException por UserFriendlyException.
            // Esto hace que el test pase (porque espera este tipo) y que el frontend muestre el mensaje bonito.
            throw new UserFriendlyException("Ya has calificado este lugar.");
        }

        var normalizedComment = string.IsNullOrWhiteSpace(input.Comment) ? null : input.Comment.Trim();

        var rating = new Rating(GuidGenerator.Create(), input.DestinationId, input.Score, normalizedComment, userId);

        await _repo.InsertAsync(rating, autoSave: true);

        return ObjectMapper.Map<Rating, RatingDto>(rating);
    }

    public async Task<ListResultDto<RatingDto>> GetByDestinationAsync(int destinationId)
    {
        var list = await (await _repo.GetQueryableAsync())
            .Where(r => r.DestinationId == destinationId)
            .OrderByDescending(r => r.CreationTime)
            .ToListAsync();

        return new ListResultDto<RatingDto>(
            ObjectMapper.Map<List<Rating>, List<RatingDto>>(list)
        );
    }

    public async Task<RatingDto?> GetMyForDestinationAsync(int destinationId)
    {
        if (CurrentUser.Id == null) return null;

        var entity = await (await _repo.GetQueryableAsync())
            .Where(r => r.DestinationId == destinationId && r.UserId == CurrentUser.Id.Value) // <--- CORRECCIÓN LÓGICA IMPORTANTE
            .SingleOrDefaultAsync();

        return entity == null ? null : ObjectMapper.Map<Rating, RatingDto>(entity);
    }
}