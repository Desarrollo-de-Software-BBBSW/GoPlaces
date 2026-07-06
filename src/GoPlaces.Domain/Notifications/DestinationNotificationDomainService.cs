using GoPlaces.Follows;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace GoPlaces.Notifications
{
    // Encapsula la creación de notificaciones por cambios en un destino. Vive en el dominio
    // (sin [Authorize]) para que pueda ser invocada tanto desde un AppService autenticado
    // (NotificationAppService) como desde procesos internos sin usuario logueado, como
    // EventSyncBackgroundWorker.
    public class DestinationNotificationDomainService : DomainService
    {
        private readonly IRepository<Notification, Guid> _notificationRepository;
        private readonly IRepository<FollowList, Guid> _followListRepository;

        public DestinationNotificationDomainService(
            IRepository<Notification, Guid> notificationRepository,
            IRepository<FollowList, Guid> followListRepository)
        {
            _notificationRepository = notificationRepository;
            _followListRepository = followListRepository;
        }

        public virtual async Task NotifyDestinationChangeAsync(Guid destinationId, string changeDescription)
        {
            // 1. Traemos TODAS las listas de favoritos de todos los usuarios (incluyendo sus items adentro)
            var queryable = await _followListRepository.WithDetailsAsync(x => x.Items);

            // 2. Filtramos solo las listas que tengan guardado este destino en particular
            var affectedLists = queryable
                .Where(list => list.Items.Any(item => item.DestinationId == destinationId))
                .ToList();

            // 3. Le creamos y guardamos una notificación a cada dueño de esas listas
            foreach (var list in affectedLists)
            {
                var notification = new Notification(
                    GuidGenerator.Create(),
                    list.OwnerUserId,
                    "¡Actualización en tu destino favorito!", // Título estándar
                    changeDescription,
                    destinationId
                );

                await _notificationRepository.InsertAsync(notification, autoSave: true);
            }
        }
    }
}
