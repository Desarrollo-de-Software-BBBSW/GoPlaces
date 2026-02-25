using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GoPlaces.Follows;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Security.Claims;
using Xunit;

namespace GoPlaces.Notifications
{
    [Collection(GoPlacesTestConsts.CollectionDefinitionName)]
    public class NotificationAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly INotificationAppService _notificationAppService;
        private readonly IFollowAppService _followAppService;
        private readonly IRepository<Notification, Guid> _notificationRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

        public NotificationAppService_Tests()
        {
            _notificationAppService = GetRequiredService<INotificationAppService>();
            _followAppService = GetRequiredService<IFollowAppService>();
            _notificationRepository = GetRequiredService<IRepository<Notification, Guid>>();
            _guidGenerator = GetRequiredService<IGuidGenerator>();
            _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
        }

        private IDisposable ChangeUserContext(Guid userId, string userName)
        {
            var claims = new[]
            {
                new Claim(Volo.Abp.Security.Claims.AbpClaimTypes.UserId, userId.ToString()),
                new Claim(Volo.Abp.Security.Claims.AbpClaimTypes.UserName, userName)
            };
            return _currentPrincipalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));
        }

        [Fact]
        public async Task Should_Notify_Only_Users_Who_Follow_Destination()
        {
            var userLucasId = _guidGenerator.Create();
            var userMarcosId = _guidGenerator.Create();
            var destinationId = _guidGenerator.Create();

            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(userLucasId, "lucas"))
                {
                    await _followAppService.SaveDestinationAsync(new SaveOrRemoveInputDto { DestinationId = destinationId });
                }
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var input = new NotifyDestinationChangeInputDto
                {
                    DestinationId = destinationId,
                    ChangeDescription = "¡Hay un nuevo festival gastronómico este fin de semana!"
                };
                await _notificationAppService.NotifyDestinationChangeAsync(input);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var lucasNotifications = await _notificationRepository.GetListAsync(n => n.UserId == userLucasId);
                var marcosNotifications = await _notificationRepository.GetListAsync(n => n.UserId == userMarcosId);

                lucasNotifications.Count.ShouldBe(1);
                lucasNotifications[0].Message.ShouldContain("festival gastronómico");
                lucasNotifications[0].IsRead.ShouldBeFalse();

                marcosNotifications.Count.ShouldBe(0);
            });
        }

        // 👇 NUEVA PRUEBA: Verifica el ciclo completo de lectura y no lectura
        [Fact]
        public async Task Should_Change_Notification_Read_State()
        {
            var userId = _guidGenerator.Create();
            var destinationId = _guidGenerator.Create();
            Guid notificationId = Guid.Empty;

            // 1. Creamos una notificación (por defecto IsRead es false)
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(userId, "traveler_lucas"))
                {
                    var notification = new Notification(
                        _guidGenerator.Create(),
                        userId,
                        "Test Título",
                        "Test Mensaje",
                        destinationId
                    );
                    var inserted = await _notificationRepository.InsertAsync(notification, autoSave: true);
                    notificationId = inserted.Id;
                }
            });

            // 2. Act: El usuario la marca como LEÍDA (true)
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(userId, "traveler_lucas"))
                {
                    await _notificationAppService.ChangeReadStateAsync(notificationId, true);
                }
            });

            // 3. Assert: Verificamos que ahora sea true
            await WithUnitOfWorkAsync(async () =>
            {
                var notification = await _notificationRepository.GetAsync(notificationId);
                notification.IsRead.ShouldBeTrue();
            });

            // 4. Act: El usuario se arrepiente y la marca como NO LEÍDA (false)
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(userId, "traveler_lucas"))
                {
                    await _notificationAppService.ChangeReadStateAsync(notificationId, false);
                }
            });

            // 5. Assert: Verificamos que volvió a ser false
            await WithUnitOfWorkAsync(async () =>
            {
                var notification = await _notificationRepository.GetAsync(notificationId);
                notification.IsRead.ShouldBeFalse();
            });
        }
    }
}