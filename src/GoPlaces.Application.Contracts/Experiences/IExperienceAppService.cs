using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;

namespace GoPlaces.Experiences
{
    public interface IExperienceAppService :
        ICrudAppService<
            ExperienceDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateExperienceDto>
    {
        Task<ListResultDto<ExperienceDto>> GetOtherUsersExperiencesAsync(Guid destinationId);

        // ✅ NUEVO MÉTODO: Filtrar por valoración
        Task<ListResultDto<ExperienceDto>> GetExperiencesByRatingAsync(string rating);
    }
}