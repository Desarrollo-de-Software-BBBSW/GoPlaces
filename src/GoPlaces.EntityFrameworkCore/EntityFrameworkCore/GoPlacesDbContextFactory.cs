using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GoPlaces.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class GoPlacesDbContextFactory : IDesignTimeDbContextFactory<GoPlacesDbContext>
{
    public GoPlacesDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        GoPlacesEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<GoPlacesDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new GoPlacesDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../GoPlaces.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
