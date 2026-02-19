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

        // ✅ NUEVA LÓGICA: Filtrar por valoración (Positiva, Negativa, Neutra)
        public async Task<ListResultDto<ExperienceDto>> GetExperiencesByRatingAsync(string rating)
        {
            // Buscamos ignorando mayúsculas/minúsculas para ser más flexibles
            var experiences = await Repository.GetListAsync(x => x.Rating.ToLower() == rating.ToLower());

            var experienceDtos = ObjectMapper.Map<List<Experience>, List<ExperienceDto>>(experiences);
            return new ListResultDto<ExperienceDto>(experienceDtos);
        }
    }
}