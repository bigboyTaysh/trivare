using Microsoft.EntityFrameworkCore;
using Trivare.Domain.Entities;

namespace Trivare.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<Transport> Transport { get; set; }
    public DbSet<Accommodation> Accommodations { get; set; }
    public DbSet<Day> Days { get; set; }
    public DbSet<Place> Places { get; set; }
    public DbSet<DayAttraction> DayAttractions { get; set; }
    public DbSet<Domain.Entities.File> Files { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User and Role Configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordSalt).IsRequired().HasMaxLength(128);
            entity.Property(e => e.PasswordResetToken).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.HasOne(e => e.User).WithMany(u => u.UserRoles).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Role).WithMany(r => r.UserRoles).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        // Trip Planning Configuration
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Destination).HasMaxLength(255);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.User).WithMany(u => u.Trips).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Transport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.HasOne(e => e.Trip).WithMany(t => t.Transports).HasForeignKey(e => e.TripId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Type).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DepartureLocation).HasMaxLength(255);
            entity.Property(e => e.ArrivalLocation).HasMaxLength(255);
            entity.Property(e => e.Notes).HasMaxLength(2000);
        });

        modelBuilder.Entity<Accommodation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.HasOne(e => e.Trip).WithOne(t => t.Accommodation).HasForeignKey<Accommodation>(e => e.TripId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.TripId).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);
        });

        modelBuilder.Entity<Day>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.HasOne(e => e.Trip).WithMany(t => t.Days).HasForeignKey(e => e.TripId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Notes).HasMaxLength(2000);
        });

        // Places and Attractions Configuration
        modelBuilder.Entity<Place>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.GooglePlaceId).HasMaxLength(255);
            entity.HasIndex(e => e.GooglePlaceId).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FormattedAddress).HasMaxLength(500);
            entity.Property(e => e.Website).HasMaxLength(500);
            entity.Property(e => e.GoogleMapsLink).HasMaxLength(500);
            entity.Property(e => e.OpeningHoursText).HasMaxLength(1000);
            entity.Property(e => e.IsManuallyAdded).HasDefaultValue(false);
        });

        modelBuilder.Entity<DayAttraction>(entity =>
        {
            entity.HasKey(e => new { e.DayId, e.PlaceId });
            entity.HasOne(e => e.Day).WithMany(d => d.DayAttractions).HasForeignKey(e => e.DayId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Place).WithMany(p => p.DayAttractions).HasForeignKey(e => e.PlaceId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.IsVisited).HasDefaultValue(false);
        });

        // File Storage and Auditing Configuration
        modelBuilder.Entity<Domain.Entities.File>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1024);
            entity.Property(e => e.FileType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Trip).WithMany(t => t.Files).HasForeignKey(e => e.TripId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Transport).WithMany(t => t.Files).HasForeignKey(e => e.TransportId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Accommodation).WithMany(a => a.Files).HasForeignKey(e => e.AccommodationId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Day).WithMany(d => d.Files).HasForeignKey(e => e.DayId).OnDelete(DeleteBehavior.NoAction);

            entity.ToTable(tb => tb.HasCheckConstraint("CK_Files_PolymorphicLink",
                "(CASE WHEN TransportId IS NOT NULL THEN 1 ELSE 0 END) + " +
                "(CASE WHEN AccommodationId IS NOT NULL THEN 1 ELSE 0 END) + " +
                "(CASE WHEN DayId IS NOT NULL THEN 1 ELSE 0 END) <= 1"));
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EventTimestamp).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.EventType, e.EventTimestamp });
        });
    }
}
