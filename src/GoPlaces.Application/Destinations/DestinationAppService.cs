using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace GoPlaces.Destinations
{
    public class DestinationAppService :
        CrudAppService<
            Destination,
            DestinationDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateDestinationDto>,
        IDestinationAppService
    {
        private readonly IRepository<Destination, Guid> _repository;

        public DestinationAppService(IRepository<Destination, Guid> repository)
            : base(repository)
        {
            _repository = repository;
        }

        // ✅ 1. ESTÁNDAR (Para el Frontend y ABP)
        // Sobreescribimos el método base para agregar validaciones si quieres
        public override async Task<DestinationDto> CreateAsync(CreateUpdateDestinationDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                throw new ArgumentException("Destination name cannot be empty");
            }

            // Aquí podrías agregar la lógica de "evitar duplicados" si quisieras
            // var existing = ...

            // Llama a la lógica base de ABP (Map -> Insert -> Save)
            return await base.CreateAsync(input);
        }

        // ✅ 2. COMPATIBILIDAD (Para tus Tests viejos)
        // Mantenemos este método para que nada explote si alguien llama a "Crear"
        public async Task<DestinationDto> Crear(CreateUpdateDestinationDto input)
        {
            // Simplemente redirige al método estándar
            return await CreateAsync(input);
        }

        // ✅ 3. EXTRA (Para listados simples)
        public async Task<List<DestinationDto>> GetAllDestinationsAsync()
        {
            var destinations = await _repository.GetListAsync();
            return ObjectMapper.Map<List<Destination>, List<DestinationDto>>(destinations);
        }
    }
}