using GoPlaces.Ratings;
using GoPlaces.Destinations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace GoPlaces.Cities
{
    public class CityAppService : ApplicationService, ICityAppService
    {
        // 1. Declaración de variables (Repositorios)
        private readonly ICitySearchService _citySearchService;
        private readonly IRepository<Destination, Guid> _destinationRepository; // ✅ Nombre estandarizado
        private readonly IRepository<Rating, Guid> _ratingRepository;

        // 2. Constructor (Inyección de dependencias)
        public CityAppService(
            ICitySearchService citySearchService,
            IRepository<Destination, Guid> destinationRepository, // ✅ Coincide con la variable
            IRepository<Rating, Guid> ratingRepository)
        {
            _citySearchService = citySearchService;
            _destinationRepository = destinationRepository; // ✅ Asignación correcta
            _ratingRepository = ratingRepository;
        }

        public async Task<CitySearchResultDto> SearchCitiesAsync(CitySearchRequestDto request)
        {
            return await _citySearchService.SearchCitiesAsync(request);
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