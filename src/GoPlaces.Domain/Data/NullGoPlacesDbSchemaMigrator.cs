using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace GoPlaces.Data;

/* This is used if database provider does't define
 * IGoPlacesDbSchemaMigrator implementation.
 */
public class NullGoPlacesDbSchemaMigrator : IGoPlacesDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
