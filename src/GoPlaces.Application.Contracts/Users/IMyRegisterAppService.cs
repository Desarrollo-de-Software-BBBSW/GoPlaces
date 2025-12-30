using System.Threading.Tasks;
using Volo.Abp.Application.Services; // <--- Importante

namespace GoPlaces.Users
{
    // DEBE heredar de IApplicationService
    public interface IMyRegisterAppService : IApplicationService
    {
        Task RegisterAsync(RegisterInputDto input);
    }
}