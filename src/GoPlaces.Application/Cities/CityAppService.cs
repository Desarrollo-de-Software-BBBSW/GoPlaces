using GoPlaces.Ratings;
using GoPlaces.Destinations;
using GoPlaces.ExternalApiMetrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace GoPlaces.Cities
{
    public class CityAppService : ApplicationService, ICityAppService
    {
        private readonly ICitySearchService _citySearchService;
        private readonly IRepository<Destination, Guid> _destinationRepository;
        private readonly IRepository<Rating, Guid> _ratingRepository;
        private readonly IRepository<ExternalApiCall, Guid> _externalApiCallRepository;
        private readonly CitySearchDomainService _citySearchDomainService;

        public CityAppService(
            ICitySearchService citySearchService,
            IRepository<Destination, Guid> destinationRepository,
            IRepository<Rating, Guid> ratingRepository,
            IRepository<ExternalApiCall, Guid> externalApiCallRepository,
            CitySearchDomainService citySearchDomainService)
        {
            _citySearchService = citySearchService;
            _destinationRepository = destinationRepository;
            _ratingRepository = ratingRepository;
            _externalApiCallRepository = externalApiCallRepository;
            _citySearchDomainService = citySearchDomainService;
        }

        public Task<CitySearchResultDto> SearchCitiesAsync(CitySearchRequestDto request)
        {
            return GetListAsync(request);
        }

        public async Task<CitySearchResultDto> GetListAsync(CitySearchRequestDto request)
        {
            // Validación de lógica de negocio en la capa de dominio
            _citySearchDomainService.ValidateFilters(request?.MinPopulation);

            var endpointBuilder = new StringBuilder($"cities?namePrefix={request?.PartialName}");
            if (!string.IsNullOrWhiteSpace(request?.CountryCode))
                endpointBuilder.Append($"&countryIds={request.CountryCode}");
            if (!string.IsNullOrWhiteSpace(request?.RegionId))
                endpointBuilder.Append($"&regionCode={request.RegionId}");
            if (request?.MinPopulation.HasValue == true)
                endpointBuilder.Append($"&minPopulation={request.MinPopulation}");

            var endpoint = endpointBuilder.ToString();
            var stopwatch = Stopwatch.StartNew();
            var isSuccess = false;

            try
            {
                var result = await _citySearchService.SearchCitiesAsync(request);
                isSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al buscar ciudades con filtros avanzados.");
                throw new UserFriendlyException("Ocurrió un error al consultar la API de ciudades. Intenta nuevamente más tarde.");
            }
            finally
            {
                stopwatch.Stop();
                await _externalApiCallRepository.InsertAsync(
                    new ExternalApiCall(Guid.NewGuid(), "GeoDB", endpoint, (int)stopwatch.ElapsedMilliseconds, isSuccess),
                    autoSave: true
                );
            }
        }

        public async Task<CityDto> GetAsync(Guid id)
        {
            var entity = await _destinationRepository.FindAsync(id); // ✅ Usa _destinationRepository

            if (entity != null)
            {
                return new CityDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Country = entity.Country,
                    Rating = 0
                };
            }

            return await _citySearchService.GetByIdAsync(id);
        }

        // 3. El Método de Promedios (Group By Global)
        public async Task<List<CityDto>> GetPopularCitiesAsync()
        {
            // 1. Obtenemos las tablas
            var destinations = await _destinationRepository.GetQueryableAsync();
            var ratingsQuery = await _ratingRepository.GetQueryableAsync();

            // ⚠️ LA LÍNEA MÁGICA: .IgnoreQueryFilters()
            // Esto desactiva cualquier filtro automático (SoftDelete, MultiTenant, CreatorId).
            // Le dice a la base de datos: "Dame TODOS los registros, sin excepciones".
            var ratings = ratingsQuery.IgnoreQueryFilters();

            // 2. Agrupación y Matemática (Igual que antes, pero ahora con datos reales)
            var statsQuery = from r in ratings
                             where r.Score > 0 // Nos aseguramos que sean votos válidos
                             group r by r.DestinationId into g
                             select new
                             {
                                 DestinationId = g.Key,
                                 AverageScore = g.Average(x => x.Score)
                             };

            // 3. Ordenar y Tomar los Mejores
            var bestStats = statsQuery
                            .OrderByDescending(x => x.AverageScore)
                            .Take(6);

            // 4. Unir con los nombres de las ciudades
            var finalQuery = from s in bestStats
                             join d in destinations on s.DestinationId equals d.Id
                             select new CityDto
                             {
                                 Id = d.Id,
                                 Name = d.Name,
                                 Country = d.Country,
                                 // Convertimos el promedio a Double (según tu DTO) o Int
                                 Rating = Math.Round(s.AverageScore, 1) // Redondeo a 1 decimal
                             };

            return await AsyncExecuter.ToListAsync(finalQuery);
        }
    }
}