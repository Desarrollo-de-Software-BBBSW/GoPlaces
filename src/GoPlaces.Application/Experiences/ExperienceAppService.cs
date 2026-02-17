using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using GoPlaces.Destinations; // Necesario para verificar el destino

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

        // Sobreescribimos CreateAsync para validar que el destino exista
        public override async Task<ExperienceDto> CreateAsync(CreateUpdateExperienceDto input)
        {
            var destinationExists = await _destinationRepository.AnyAsync(x => x.Id == input.DestinationId);
            if (!destinationExists)
            {
                throw new UserFriendlyException("El destino especificado no existe.");
            }

            return await base.CreateAsync(input);
        }
    }
}