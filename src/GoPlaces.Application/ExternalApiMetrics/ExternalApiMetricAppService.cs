using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace GoPlaces.ExternalApiMetrics
{
    [Authorize] // Requiere estar logueado
    public class ExternalApiMetricAppService : ApplicationService, IExternalApiMetricAppService
    {
        private readonly IRepository<ExternalApiCall, Guid> _apiCallRepository;

        public ExternalApiMetricAppService(IRepository<ExternalApiCall, Guid> apiCallRepository)
        {
            _apiCallRepository = apiCallRepository;
        }

        public async Task<List<ApiUsageMetricDto>> GetMetricsAsync()
        {
            // 1. SEGURIDAD: Verificamos que el usuario tenga el rol "admin"
            var isAdmin = CurrentUser.Roles.Contains("admin");
            if (!isAdmin)
            {
                throw new UnauthorizedAccessException("Acceso denegado: Solo los administradores pueden ver las métricas del sistema.");
            }

            // 2. Traemos todos los registros de la base de datos
            var calls = await _apiCallRepository.GetListAsync();

            // 3. Agrupamos por el nombre de la API (ej: "Amadeus", "GoogleMaps") y calculamos estadísticas
            var metrics = calls.GroupBy(x => x.ApiName)
                .Select(group => new ApiUsageMetricDto
                {
                    ApiName = group.Key,
                    TotalCalls = group.Count(),
                    SuccessfulCalls = group.Count(x => x.IsSuccess),
                    FailedCalls = group.Count(x => !x.IsSuccess),
                    AverageDurationMs = Math.Round(group.Average(x => x.DurationMs), 2)
                })
                .ToList();

            return metrics;
        }

        // Método auxiliar temporal para poder probar el gráfico llenándolo de datos
        [AllowAnonymous] // Lo dejamos abierto para no pelear con Swagger al cargar datos
        public async Task LogCallAsync(string apiName, string endpoint, int durationMs, bool isSuccess)
        {
            var apiCall = new ExternalApiCall(
                GuidGenerator.Create(),
                apiName,
                endpoint,
                durationMs,
                isSuccess
            );
            await _apiCallRepository.InsertAsync(apiCall, autoSave: true);
        }
    }
}