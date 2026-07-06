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
        private readonly DestinationNotificationDomainService _destinationNotificationDomainService;

        public NotificationAppService(
            IRepository<Notification, Guid> notificationRepository,
            DestinationNotificationDomainService destinationNotificationDomainService)
        {
            _notificationRepository = notificationRepository;
            _destinationNotificationDomainService = destinationNotificationDomainService;
        }

        public async Task NotifyDestinationChangeAsync(NotifyDestinationChangeInputDto input)
        {
            await _destinationNotificationDomainService.NotifyDestinationChangeAsync(input.DestinationId, input.ChangeDescription);
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