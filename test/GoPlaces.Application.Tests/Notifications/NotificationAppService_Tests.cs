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
            // Arrange
            var userLucasId = _guidGenerator.Create(); // Fan del destino
            var userMarcosId = _guidGenerator.Create(); // A Marcos no le interesa
            var destinationId = _guidGenerator.Create();

            // Lucas agrega el destino a favoritos
            await WithUnitOfWorkAsync(async () =>
            {
                using (ChangeUserContext(userLucasId, "lucas"))
                {
                    await _followAppService.SaveDestinationAsync(new SaveOrRemoveInputDto { DestinationId = destinationId });
                }
            });

            // Act: Disparamos la notificación de que hay un evento nuevo en ese destino
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new NotifyDestinationChangeInputDto
                {
                    DestinationId = destinationId,
                    ChangeDescription = "¡Hay un nuevo festival gastronómico este fin de semana!"
                };
                await _notificationAppService.NotifyDestinationChangeAsync(input);
            });

            // Assert: Verificamos quién recibió la notificación
            await WithUnitOfWorkAsync(async () =>
            {
                var lucasNotifications = await _notificationRepository.GetListAsync(n => n.UserId == userLucasId);
                var marcosNotifications = await _notificationRepository.GetListAsync(n => n.UserId == userMarcosId);

                // Lucas debe tener 1 notificación con el texto correcto
                lucasNotifications.Count.ShouldBe(1);
                lucasNotifications[0].Message.ShouldContain("festival gastronómico");
                lucasNotifications[0].IsRead.ShouldBeFalse();

                // Marcos debe tener 0 notificaciones
                marcosNotifications.Count.ShouldBe(0);
            });
        }
    }
}