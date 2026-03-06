using GoPlaces.Users;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data; // 👈 NECESARIO para leer propiedades extra (PhotoUrl)
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace GoPlaces.Users;


public class PublicUserAppService : GoPlacesAppService, IPublicUserLookupAppService
{
    protected IIdentityUserRepository UserRepository { get; }

    public PublicUserAppService(IIdentityUserRepository userRepository)
    {
        UserRepository = userRepository;
    }

    public virtual async Task<PublicUserProfileDto> GetByUserNameAsync(string userName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new UserFriendlyException("El nombre de usuario no puede estar vacío.");

            var user = await UserRepository.FindByNormalizedUserNameAsync(userName.ToUpper(), includeDetails: true);

            if (user == null)
                throw new UserFriendlyException($"No se encontró el usuario: {userName}");

            return new PublicUserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Name = user.Name ?? string.Empty,
                Surname = user.Surname ?? string.Empty,
                PhotoUrl = user.GetProperty<string>("PhotoUrl")
            };
        }
        catch (UserFriendlyException)
        {
            throw; // estas las dejamos pasar normal
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"Error detallado: {ex.Message} | {ex.InnerException?.Message}");
        }
    }
}