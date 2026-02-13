using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace GoPlaces.Destinations;

public class Coordinates : ValueObject
{
    // EF Core necesita 'private set' para poder rellenar estos datos al leer de la DB.
    // Si solo pones { get; }, EF Core a veces falla al materializar la entidad.
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    // Constructor privado vacío requerido por EF Core/JSON Serializers
    private Coordinates() { }

    public Coordinates(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    // Esto es obligatorio en ABP ValueObject para que la comparación (==) funcione bien
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Latitude;
        yield return Longitude;
    }
}