using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Data; // 👈 NECESARIO para leer propiedades extra (PhotoUrl)
using GoPlaces.Users;

namespace GoPlaces.Users;

[ExposeServices(typeof(IPublicUserLookupAppService))]
public class PublicUserAppService : GoPlacesAppService, IPublicUserLookupAppService
{
    protected IIdentityUserRepository UserRepository { get; }

    public PublicUserAppService(IIdentityUserRepository userRepository)
    {
        UserRepository = userRepository;
    }

    public virtual async Task<PublicUserProfileDto> GetByUserNameAsync(string userName)
    {
        // includeDetails: true asegura que traiga todo
        var user = await UserRepository.FindByNormalizedUserNameAsync(userName.ToUpper(), includeDetails: true);

        if (user == null)
        {
            throw new UserFriendlyException($"No se encontró el usuario: {userName}");
        }

        return new PublicUserProfileDto
        {
            UserName = user.UserName,
            Name = user.Name,
            Surname = user.Surname, // Agregamos el apellido también por si acaso

            // 👇 ESTA ES LA LÍNEA QUE FALTABA Y CAUSABA EL ERROR "but was null"
            PhotoUrl = user.GetProperty<string>("PhotoUrl")
        };
    }
}