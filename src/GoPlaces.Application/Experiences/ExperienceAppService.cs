using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Authorization;
using GoPlaces.Destinations;

namespace GoPlaces.Experiences
{
    public class ExperienceAppService :
        CrudAppService<
            Experience,
            ExperienceDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateExperienceDto>,
        IExperienceAppService
    {
        private readonly IRepository<Destination, Guid> _destinationRepository;

        public ExperienceAppService(
            IRepository<Experience, Guid> repository,
            IRepository<Destination, Guid> destinationRepository)
            : base(repository)
        {
            _destinationRepository = destinationRepository;
        }

        public override async Task<ExperienceDto> CreateAsync(CreateUpdateExperienceDto input)
        {
            var destinationExists = await _destinationRepository.AnyAsync(x => x.Id == input.DestinationId);
            if (!destinationExists)
            {
                throw new UserFriendlyException("El destino especificado no existe.");
            }
            return await base.CreateAsync(input);
        }

        public override async Task<ExperienceDto> UpdateAsync(Guid id, CreateUpdateExperienceDto input)
        {
            var experience = await Repository.GetAsync(id);

            if (experience.CreatorId != CurrentUser.Id)
            {
                throw new AbpAuthorizationException("No tienes permiso para editar esta experiencia. Solo el creador puede hacerlo.");
            }

            var destinationExists = await _destinationRepository.AnyAsync(x => x.Id == input.DestinationId);
            if (!destinationExists)
            {
                throw new UserFriendlyException("El destino especificado no existe.");
            }

            return await base.UpdateAsync(id, input);
        }

        public override async Task DeleteAsync(Guid id)
        {
            var experience = await Repository.GetAsync(id);

            if (experience.CreatorId != CurrentUser.Id)
            {
                throw new AbpAuthorizationException("No tienes permiso para eliminar esta experiencia. Solo el creador puede hacerlo.");
            }

            await base.DeleteAsync(id);
        }

        public async Task<ListResultDto<ExperienceDto>> GetOtherUsersExperiencesAsync(Guid destinationId)
        {
            var currentUserId = CurrentUser.Id;
            var experiences = await Repository.GetListAsync(x =>
                x.DestinationId == destinationId &&
                x.CreatorId != currentUserId);

            var experienceDtos = ObjectMapper.Map<List<Experience>, List<ExperienceDto>>(experiences);
            return new ListResultDto<ExperienceDto>(experienceDtos);
        }

        public async Task<ListResultDto<ExperienceDto>> GetExperiencesByRatingAsync(string rating)
        {
            var experiences = await Repository.GetListAsync(x => x.Rating.ToLower() == rating.ToLower());

            var experienceDtos = ObjectMapper.Map<List<Experience>, List<ExperienceDto>>(experiences);
            return new ListResultDto<ExperienceDto>(experienceDtos);
        }

        // ✅ NUEVA LÓGICA: Buscar por palabra clave en Título y Descripción
        public async Task<ListResultDto<ExperienceDto>> SearchExperiencesByKeywordAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new ListResultDto<ExperienceDto>(); // Devolvemos lista vacía si no envían nada
            }

            var keywordLower = keyword.ToLower();

            // Buscamos si la palabra está en el título O en la descripción
            var experiences = await Repository.GetListAsync(x =>
                x.Title.ToLower().Contains(keywordLower) ||
                x.Description.ToLower().Contains(keywordLower));

            var experienceDtos = ObjectMapper.Map<List<Experience>, List<ExperienceDto>>(experiences);
            return new ListResultDto<ExperienceDto>(experienceDtos);
        }
    }
}