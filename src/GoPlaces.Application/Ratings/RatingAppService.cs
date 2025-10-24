using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace GoPlaces.Ratings;

[Authorize] // exige JWT
public class RatingAppService : ApplicationService, IRatingAppService
{
    private readonly IRepository<Rating, Guid> _repo;

    public RatingAppService(IRepository<Rating, Guid> repo)
    {
        _repo = repo;
    }

    public async Task<RatingDto> CreateAsync(CreateRatingDto input)
    {
        if (input.Score < 1 || input.Score > 5)
        {
            throw new BusinessException("Rating.ScoreOutOfRange")
                .WithData("Score", input.Score);
        }

        var userId = CurrentUser.GetId();

        // Unicidad por (UserId, DestinationId)
        var exists = await (await _repo.GetQueryableAsync())
            .AnyAsync(r => r.DestinationId == input.DestinationId && r.UserId == userId);

        if (exists)
        {
            throw new BusinessException("Rating.AlreadyExists")
                .WithData("DestinationId", input.DestinationId);
        }

        // Normalizar comentario
        var normalizedComment = string.IsNullOrWhiteSpace(input.Comment)
            ? null
            : input.Comment.Trim();

        var rating = new Rating(
            GuidGenerator.Create(),
            input.DestinationId,
            input.Score,
            normalizedComment,
            userId
        );

        await _repo.InsertAsync(rating, autoSave: true);

        return ObjectMapper.Map<Rating, RatingDto>(rating);
    }

    public async Task<ListResultDto<RatingDto>> GetByDestinationAsync(Guid destinationId)
    {
        var query = await _repo.GetQueryableAsync();

        var list = await query
            .Where(r => r.DestinationId == destinationId)
            // si Rating hereda de AuditedAggregateRoot<Guid>, dejá la línea de abajo:
            .OrderByDescending(r => r.CreationTime)
            .ToListAsync();

        return new ListResultDto<RatingDto>(
            ObjectMapper.Map<List<Rating>, List<RatingDto>>(list)
        );
    }

    public async Task<RatingDto?> GetMyForDestinationAsync(Guid destinationId)
    {
        var userId = CurrentUser.GetId();

        var query = await _repo.GetQueryableAsync();
        var entity = await query
            .Where(r => r.DestinationId == destinationId && r.UserId == userId)
            .SingleOrDefaultAsync(); // hay índice único (DestinationId, UserId)

        return entity == null ? null : ObjectMapper.Map<Rating, RatingDto>(entity);
    }
}
