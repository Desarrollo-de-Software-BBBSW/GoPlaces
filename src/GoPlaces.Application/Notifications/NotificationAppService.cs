using GoPlaces.Follows;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp; // Necesario para UserFriendlyException
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace GoPlaces.Notifications

{
    [Authorize]
    public class NotificationAppService : ApplicationService, INotificationAppService
    {
        private readonly IRepository<Notification, Guid> _notificationRepository;
        private readonly IRepository<FollowList, Guid> _followListRepository;

        public NotificationAppService(
            IRepository<Notification, Guid> notificationRepository,
            IRepository<FollowList, Guid> followListRepository)
        {
            _notificationRepository = notificationRepository;
            _followListRepository = followListRepository;
        }

        public async Task NotifyDestinationChangeAsync(NotifyDestinationChangeInputDto input)
        {
            // 1. Traemos TODAS las listas de favoritos de todos los usuarios (incluyendo sus items adentro)
            var queryable = await _followListRepository.WithDetailsAsync(x => x.Items);

            // 2. Filtramos solo las listas que tengan guardado este destino en particular
            var affectedLists = queryable
                .Where(list => list.Items.Any(item => item.DestinationId == input.DestinationId))
                .ToList();

            // 3. Le creamos y guardamos una notificación a cada dueño de esas listas
            foreach (var list in affectedLists)
            {
                var notification = new Notification(
                    GuidGenerator.Create(),
                    list.OwnerUserId,
                    "¡Actualización en tu destino favorito!", // Título estándar
                    input.ChangeDescription, // El mensaje que nos mandaron
                    input.DestinationId
                );

                await _notificationRepository.InsertAsync(notification, autoSave: true);
            }
        }

        public async Task<List<NotificationDto>> GetMyNotificationsAsync()
        {
            var userId = CurrentUser.Id.Value;

            // Buscamos las notificaciones del usuario actual, ordenadas por las más nuevas
            var notifications = await _notificationRepository.GetListAsync(
                n => n.UserId == userId
            );

            return notifications
                .OrderByDescending(n => n.CreationTime)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreationTime = n.CreationTime
                }).ToList();
        }

        // 👇 NUEVO MÉTODO: Cambiar estado de lectura de la notificación
        public async Task ChangeReadStateAsync(Guid id, bool isRead)
        {
            // 1. Buscamos la notificación
            var notification = await _notificationRepository.GetAsync(id);

            // 2. Seguridad: Verificamos que sea del usuario actual
            if (notification.UserId != CurrentUser.Id.Value)
            {
                throw new UserFriendlyException("No tienes permiso para modificar esta notificación.");
            }

            // 3. Cambiamos el estado
            notification.SetReadState(isRead);

            // 4. Guardamos los cambios
            await _notificationRepository.UpdateAsync(notification, autoSave: true);
        }
    }
}