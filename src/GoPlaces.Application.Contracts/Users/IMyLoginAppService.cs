using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GoPlaces.Users
{
    public interface IMyLoginAppService : IApplicationService
    {
        // Devolvemos true si el login es exitoso, o lanzamos excepción si falla
        Task<bool> LoginAsync(LoginInputDto input);
    }
}