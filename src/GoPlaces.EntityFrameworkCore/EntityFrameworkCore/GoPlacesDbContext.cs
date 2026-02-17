using GoPlaces.Destinations;
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
    // ✅ CORRECTO: Una sola definición para Destinations
    public DbSet<Destination> Destinations { get; set; }

    // ✅ IMPORTANTE: Descomenta esto. Sin esto, RatingAppService falla al iniciar.
    public DbSet<Rating> Ratings { get; set; }

    public DbSet<GoPlaces.Follows.FollowList> FollowLists { get; set; }
    public DbSet<GoPlaces.Follows.FollowListItem> FollowListItems { get; set; }

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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureBlobStoring();

        // Configuración de Destination
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

        // Configuración de Ratings (¡DESCOMENTAR!)
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

        // FollowLists... (El resto igual)
        builder.Entity<GoPlaces.Follows.FollowList>(b =>
        {
            b.ToTable(GoPlacesConsts.DbTablePrefix + "FollowLists", GoPlacesConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(GoPlaces.Follows.FollowList.NameMaxLength);
            b.Property(x => x.Description).HasMaxLength(GoPlaces.Follows.FollowList.DescriptionMaxLength);
            b.Property(x => x.LastUpdatedDate).HasColumnName("last_updated_date").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            b.HasIndex(x => new { x.OwnerUserId, x.IsDefault });
            b.HasIndex(x => x.OwnerUserId).IsUnique();
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
    }
}