using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace GoPlaces.Follows
{
    [Authorize] // Solo usuarios logueados pueden tener favoritos
    public class FollowAppService : ApplicationService, IFollowAppService
    {
        private readonly IFollowListRepository _followListRepository;

        public FollowAppService(IFollowListRepository followListRepository)
        {
            _followListRepository = followListRepository;
        }

        public async Task<SavedDestinationDto> SaveDestinationAsync(SaveOrRemoveInputDto input)
        {
            var userId = CurrentUser.Id.Value;

            // 1. Buscamos si el usuario ya tiene su lista por defecto
            var list = await _followListRepository.FindDefaultByOwnerAsync(userId);

            // 2. Si no la tiene, se la creamos en el momento
            if (list == null)
            {
                list = new FollowList(
                    GuidGenerator.Create(),
                    userId,
                    "Mis Favoritos",
                    isDefault: true
                );
                await _followListRepository.InsertAsync(list, autoSave: true);
            }

            // 3. Agregamos el destino (la clase FollowList ya valida que no esté duplicado)
            var item = list.AddDestination(input.DestinationId);

            // 4. Guardamos los cambios
            await _followListRepository.UpdateAsync(list, autoSave: true);

            // 5. Retornamos el DTO
            return new SavedDestinationDto
            {
                Id = item.Id,
                DestinationId = item.DestinationId,
                CreationTime = item.CreationTime
            };
        }

        // 👇 NUEVO MÉTODO: Eliminar destino de favoritos
        public async Task RemoveDestinationAsync(SaveOrRemoveInputDto input)
        {
            var userId = CurrentUser.Id.Value;

            // 1. Buscamos la lista del usuario actual (esto ya garantiza que no pueda tocar la de otros)
            var list = await _followListRepository.FindDefaultByOwnerAsync(userId);

            // 2. Si no tiene lista, lanzamos una excepción amigable
            if (list == null)
            {
                throw new UserFriendlyException("No tienes una lista de favoritos.");
            }

            // 3. Si la tiene, usamos el método de dominio para remover el destino
            list.RemoveDestination(input.DestinationId);

            // 4. Guardamos los cambios
            await _followListRepository.UpdateAsync(list, autoSave: true);
        }
    }
}