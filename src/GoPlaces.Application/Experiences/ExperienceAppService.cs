using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Authorization; // Necesario para AbpAuthorizationException
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

        // ✅ Lógica de Creación
        public override async Task<ExperienceDto> CreateAsync(CreateUpdateExperienceDto input)
        {
            var destinationExists = await _destinationRepository.AnyAsync(x => x.Id == input.DestinationId);
            if (!destinationExists)
            {
                throw new UserFriendlyException("El destino especificado no existe.");
            }

            return await base.CreateAsync(input);
        }

        // ✅ Lógica de Edición Segura
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

        // ✅ Lógica de Eliminación Segura
        public override async Task DeleteAsync(Guid id)
        {
            var experience = await Repository.GetAsync(id);

            if (experience.CreatorId != CurrentUser.Id)
            {
                throw new AbpAuthorizationException("No tienes permiso para eliminar esta experiencia. Solo el creador puede hacerlo.");
            }

            await base.DeleteAsync(id);
        }

        // ✅ NUEVA LÓGICA: Consultar experiencias de otros en un destino
        public async Task<ListResultDto<ExperienceDto>> GetOtherUsersExperiencesAsync(Guid destinationId)
        {
            var currentUserId = CurrentUser.Id;

            // Obtenemos la lista filtrando por destino y excluyendo mis propias experiencias
            var experiences = await Repository.GetListAsync(x =>
                x.DestinationId == destinationId &&
                x.CreatorId != currentUserId);

            // Mapeamos de Entidad a DTO para enviarlo al Front
            var experienceDtos = ObjectMapper.Map<List<Experience>, List<ExperienceDto>>(experiences);

            return new ListResultDto<ExperienceDto>(experienceDtos);
        }
    }
}