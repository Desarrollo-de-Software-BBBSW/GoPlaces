using System;
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
            // 1. Recuperamos la entidad original de la BD
            var experience = await Repository.GetAsync(id);

            // 2. Verificamos si el usuario actual es el DUEÑO
            // Si el CreatorId es null (creado por sistema) o diferente al usuario actual...
            if (experience.CreatorId != CurrentUser.Id)
            {
                throw new AbpAuthorizationException("No tienes permiso para editar esta experiencia. Solo el creador puede hacerlo.");
            }

            // 3. Validamos que el destino siga existiendo (por si intentan moverla a un ID falso)
            var destinationExists = await _destinationRepository.AnyAsync(x => x.Id == input.DestinationId);
            if (!destinationExists)
            {
                throw new UserFriendlyException("El destino especificado no existe.");
            }

            // 4. Si todo está bien, dejamos que ABP haga el update estándar
            return await base.UpdateAsync(id, input);
        }

        // ✅ NUEVA LÓGICA: Eliminación Segura
        public override async Task DeleteAsync(Guid id)
        {
            // 1. Buscamos la experiencia en la base de datos
            var experience = await Repository.GetAsync(id);

            // 2. Verificamos si el usuario actual es el DUEÑO
            if (experience.CreatorId != CurrentUser.Id)
            {
                throw new AbpAuthorizationException("No tienes permiso para eliminar esta experiencia. Solo el creador puede hacerlo.");
            }

            // 3. Si es el dueño, procedemos con el borrado estándar de ABP
            await base.DeleteAsync(id);
        }
    }
}