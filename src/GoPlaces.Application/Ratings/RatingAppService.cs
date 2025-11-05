using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;                             // 👈 necesario para Where/OrderBy/SingleOrDefault
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;


namespace GoPlaces.Ratings;

[Authorize] // ← exige JWT
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
            throw new BusinessException("Rating.ScoreOutOfRange").WithData("Score", input.Score);

        var userId = CurrentUser.GetId();

        var exists = await (await _repo.GetQueryableAsync())
            .AnyAsync(r => r.DestinationId == input.DestinationId && r.UserId == userId);

        if (exists)
            throw new BusinessException("Rating.AlreadyExists").WithData("DestinationId", input.DestinationId);

        var normalizedComment = string.IsNullOrWhiteSpace(input.Comment) ? null : input.Comment.Trim();

        var rating = new Rating(GuidGenerator.Create(), input.DestinationId, input.Score, normalizedComment, userId);

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
        var userId = CurrentUser.GetId();

        var entity = await (await _repo.GetQueryableAsync())
            .Where(r => r.DestinationId == destinationId && r.UserId == userId)
            .SingleOrDefaultAsync();

        return entity == null ? null : ObjectMapper.Map<Rating, RatingDto>(entity);
    }
}
