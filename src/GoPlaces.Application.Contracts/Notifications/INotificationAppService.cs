using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GoPlaces.Notifications
{
    public interface INotificationAppService : IApplicationService
    {
        Task NotifyDestinationChangeAsync(NotifyDestinationChangeInputDto input);

        // Para que el usuario vea sus avisos
        Task<List<NotificationDto>> GetMyNotificationsAsync();

        // 👇 NUEVO MÉTODO: Cambiar estado de lectura
        Task ChangeReadStateAsync(Guid id, bool isRead);
    }
}