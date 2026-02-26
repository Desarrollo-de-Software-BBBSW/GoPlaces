using GoPlaces.Destinations;
using GoPlaces.Experiences; // 👈 AGREGADO: Importamos Experiences
using GoPlaces.ExternalApiMetrics;
using GoPlaces.Notifications;
using GoPlaces.Ratings;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.Users;

namespace GoPlaces.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ConnectionStringName("Default")]
public class GoPlacesDbContext :
    AbpDbContext<GoPlacesDbContext>,
    IIdentityDbContext
{
    // ✅ Destinations
    public DbSet<Destination> Destinations { get; set; }

    // ✅ Ratings
    public DbSet<Rating> Ratings { get; set; }

    // ✅ FollowLists
    public DbSet<GoPlaces.Follows.FollowList> FollowLists { get; set; }
    public DbSet<GoPlaces.Follows.FollowListItem> FollowListItems { get; set; }

    // ✅ EXPERIENCES (NUEVA TABLA AGREGADA)
    public DbSet<Experience> Experiences { get; set; }

    #region Entities from the modules
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    #endregion

    public GoPlacesDbContext(DbContextOptions<GoPlacesDbContext> options)
        : base(options)
    {
    }
    public DbSet<ExternalApiCall> ExternalApiCalls { get; set; }

    public DbSet<Notification> Notifications { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Módulos ABP
        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureBlobStoring();

        // 1. Configuración de DESTINATIONS
        builder.Entity<Destination>(b =>
        {
            b.ToTable(GoPlacesConsts.DbTablePrefix + "Destinations", GoPlacesConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(Destination.NameMaxLength);
            b.Property(x => x.Country).IsRequired().HasMaxLength(Destination.CountryMaxLength);
            b.Property(x => x.Population).IsRequired();
            b.Property(x => x.ImageUrl).HasColumnName("Url_Image").HasMaxLength(Destination.ImageUrlMaxLength);
            b.Property(x => x.LastUpdatedDate).HasColumnName("last_updated_date").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            b.OwnsOne(x => x.Coordinates, o =>
            {
                o.Property(p => p.Latitude).HasColumnName("Latitude");
                o.Property(p => p.Longitude).HasColumnName("Longitude");
            });
            b.HasIndex(x => x.Name);
            b.HasIndex(x => x.Country);
        });

        // 2. Configuración de RATINGS
        builder.Entity<Rating>(b =>
        {
            b.ToTable(GoPlacesConsts.DbTablePrefix + "Ratings", GoPlacesConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.DestinationId).IsRequired();
            b.Property(x => x.Score).IsRequired();
            b.Property(x => x.Comment).HasMaxLength(1000);
            b.Property(x => x.UserId).IsRequired();
            b.HasIndex(x => new { x.DestinationId, x.UserId }).IsUnique();
        });

        // 3. Configuración de EXPERIENCES (NUEVA)
        builder.Entity<Experience>(b =>
        {
            b.ToTable(GoPlacesConsts.DbTablePrefix + "Experiences", GoPlacesConsts.DbSchema);
            b.ConfigureByConvention(); // Configura Id, CreationTime, CreatorId, etc.

            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.Property(x => x.Description).HasMaxLength(2000);
            b.Property(x => x.Price).HasColumnType("decimal(18,2)"); // Importante para dinero

            // Relación con Destination
            b.HasOne<Destination>()
             .WithMany()
             .HasForeignKey(x => x.DestinationId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade); // Si borras el destino, borras sus experiencias

            b.HasIndex(x => x.DestinationId);
        });

        // 4. Configuración de FOLLOW LISTS
        builder.Entity<GoPlaces.Follows.FollowList>(b =>
        {
            b.ToTable(GoPlacesConsts.DbTablePrefix + "FollowLists", GoPlacesConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(GoPlaces.Follows.FollowList.NameMaxLength);
            b.Property(x => x.Description).HasMaxLength(GoPlaces.Follows.FollowList.DescriptionMaxLength);
            b.Property(x => x.LastUpdatedDate).HasColumnName("last_updated_date").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            b.HasIndex(x => new { x.OwnerUserId, x.IsDefault });
            b.HasIndex(x => x.OwnerUserId).IsUnique(); // ⚠️ OJO: Esto impide tener más de 1 lista por usuario. ¿Seguro que quieres esto?
        });

        builder.Entity<GoPlaces.Follows.FollowListItem>(b =>
        {
            b.ToTable(GoPlacesConsts.DbTablePrefix + "FollowListItems", GoPlacesConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.FollowListId, x.DestinationId }).IsUnique();
            b.HasOne<GoPlaces.Follows.FollowList>()
                .WithMany(x => (ICollection<GoPlaces.Follows.FollowListItem>)x.Items)
                .HasForeignKey(x => x.FollowListId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<Notification>(b =>
        {
            b.ToTable(GoPlacesConsts.DbTablePrefix + "Notifications", GoPlacesConsts.DbSchema);
            b.ConfigureByConvention();
        });
    }
}