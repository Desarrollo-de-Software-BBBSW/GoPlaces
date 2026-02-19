using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;

namespace GoPlaces.Experiences
{
    public interface IExperienceAppService :
        ICrudAppService< // ABP nos regala: Get, GetList, Create, Update, Delete
            ExperienceDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateExperienceDto>
    {
        // ✅ NUEVO MÉTODO: Consultar experiencias de otros usuarios en un destino
        Task<ListResultDto<ExperienceDto>> GetOtherUsersExperiencesAsync(Guid destinationId);
    }
}