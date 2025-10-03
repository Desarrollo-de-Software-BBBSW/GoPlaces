using System.Threading.Tasks;

namespace GoPlaces.Data;

public interface IGoPlacesDbSchemaMigrator
{
    Task MigrateAsync();
}
