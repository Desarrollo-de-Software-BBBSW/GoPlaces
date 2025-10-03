using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GoPlaces.Data;
using Volo.Abp.DependencyInjection;

namespace GoPlaces.EntityFrameworkCore;

public class EntityFrameworkCoreGoPlacesDbSchemaMigrator
    : IGoPlacesDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreGoPlacesDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the GoPlacesDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<GoPlacesDbContext>()
            .Database
            .MigrateAsync();
    }
}
