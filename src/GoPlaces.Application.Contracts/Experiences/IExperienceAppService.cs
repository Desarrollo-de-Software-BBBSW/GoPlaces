using System;
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
        // Aquí pondremos métodos extra si los necesitamos
    }
}