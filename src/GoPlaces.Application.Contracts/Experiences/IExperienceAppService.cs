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

        Task<ListResultDto<ExperienceDto>> GetExperiencesByRatingAsync(string rating);

        // ✅ NUEVO MÉTODO: Buscar experiencias por palabra clave
        Task<ListResultDto<ExperienceDto>> SearchExperiencesByKeywordAsync(string keyword);
    }
}