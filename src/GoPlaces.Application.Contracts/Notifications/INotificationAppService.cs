using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GoPlaces.Notifications
{
    public interface INotificationAppService : IApplicationService
    {
        Task NotifyDestinationChangeAsync(NotifyDestinationChangeInputDto input);

        // 👇 NUEVO: Para que el usuario vea sus avisos
        Task<List<NotificationDto>> GetMyNotificationsAsync();
    }
}