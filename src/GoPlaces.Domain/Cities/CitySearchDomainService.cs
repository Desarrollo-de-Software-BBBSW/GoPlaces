using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace GoPlaces.Cities
{
    /// <summary>
    /// Servicio de dominio que encapsula las reglas de negocio para los filtros de búsqueda de ciudades.
    /// </summary>
    public class CitySearchDomainService : DomainService
    {
        /// <summary>
        /// Valida que los filtros de búsqueda sean coherentes con las reglas de negocio.
        /// </summary>
        /// <param name="minPopulation">Población mínima. No puede ser negativa.</param>
        public void ValidateFilters(int? minPopulation)
        {
            if (minPopulation.HasValue && minPopulation.Value < 0)
            {
                throw new UserFriendlyException(
                    "La población mínima no puede ser un valor negativo.",
                    "GoPlaces:MinPopulationCannotBeNegative"
                );
            }
        }
    }
}
