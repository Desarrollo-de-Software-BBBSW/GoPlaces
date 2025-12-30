using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GoPlaces.Users;

public interface IMyRegisterAppService : IApplicationService
{
    Task RegisterAsync(RegisterInputDto input);
}