using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GoPlaces.ExternalApiMetrics
{
    public interface IExternalApiMetricAppService : IApplicationService
    {
        // Método exclusivo para administradores
        Task<List<ApiUsageMetricDto>> GetMetricsAsync();

        // Método auxiliar para que podamos inyectar datos falsos y probarlo
        Task LogCallAsync(string apiName, string endpoint, int durationMs, bool isSuccess);
    }
}