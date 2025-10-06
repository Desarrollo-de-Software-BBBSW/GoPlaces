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

namespace GoPlaces.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ConnectionStringName("Default")]
public class GoPlacesDbContext :
    AbpDbContext<GoPlacesDbContext>,
    IIdentityDbContext
{
    // 👇 Tus agregados de dominio
    public DbSet<GoPlaces.Destinations.Destination> Destinations { get; set; }
    public DbSet<GoPlaces.Follows.FollowList> FollowLists { get; set; }
    public DbSet<GoPlaces.Follows.FollowListItem> FollowListItems { get; set; }



    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext 
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext .
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
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

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureBlobStoring();

        /* Configure your own tables/entities inside here */

        // Destination + Coordinates (owned)
        builder.Entity<GoPlaces.Destinations.Destination>(b =>
        {
            b.ToTable(GoPlacesConsts.DbTablePrefix + "Destinations", GoPlacesConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(GoPlaces.Destinations.Destination.NameMaxLength);
            b.Property(x => x.Country).IsRequired().HasMaxLength(GoPlaces.Destinations.Destination.CountryMaxLength);
            b.Property(x => x.Population).IsRequired();

            // Column names as you requested
            b.Property(x => x.ImageUrl)
                .HasColumnName("Url_Image")
                .HasMaxLength(GoPlaces.Destinations.Destination.ImageUrlMaxLength);

            b.Property(x => x.LastUpdatedDate)
                .HasColumnName("last_updated_date")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();


            b.OwnsOne(x => x.Coordinates, o =>
            {
                o.Property(p => p.Latitude).HasColumnName("Latitude");
                o.Property(p => p.Longitude).HasColumnName("Longitude");
            });

            b.HasIndex(x => x.Name);
            b.HasIndex(x => x.Country);
        });


        // FollowList
        builder.Entity<GoPlaces.Follows.FollowList>(b =>
        {
            b.ToTable(GoPlacesConsts.DbTablePrefix + "FollowLists", GoPlacesConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(GoPlaces.Follows.FollowList.NameMaxLength);
            b.Property(x => x.Description).HasMaxLength(GoPlaces.Follows.FollowList.DescriptionMaxLength);
            b.Property(x => x.LastUpdatedDate)
             .HasColumnName("last_updated_date")
             .HasDefaultValueSql("CURRENT_TIMESTAMP")
             .IsRequired();


            // índices que ya tenías
            b.HasIndex(x => new { x.OwnerUserId, x.IsDefault });
            b.HasIndex(x => x.OwnerUserId).IsUnique(); // si seguís con una sola lista por usuario (MVP)
        });


        // FollowListItem
        builder.Entity<GoPlaces.Follows.FollowListItem>(b =>
        {
            b.ToTable(GoPlacesConsts.DbTablePrefix + "FollowListItems", GoPlacesConsts.DbSchema);
            b.ConfigureByConvention();

            // evita duplicados del mismo destino en la misma lista
            b.HasIndex(x => new { x.FollowListId, x.DestinationId }).IsUnique();

            b.HasOne<GoPlaces.Follows.FollowList>()
                .WithMany(x => (ICollection<GoPlaces.Follows.FollowListItem>)x.Items)
                .HasForeignKey(x => x.FollowListId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(GoPlacesConsts.DbTablePrefix + "YourEntities", GoPlacesConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }
}
