using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PersonalDetailsApi.Domain.Common;
using PersonalDetailsApi.Domain.Entities;

namespace PersonalDetailsApi.Data;

/// <summary>
/// EF Core unit of work for the SQLite database.
/// Overrides <see cref="SaveChangesAsync(CancellationToken)"/> to apply audit timestamps and soft delete
/// in one place, so no service has to remember to do it.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options, TimeProvider timeProvider) : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<EmergencyContact> EmergencyContacts => Set<EmergencyContact>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Employment> Employments => Set<Employment>();
    public DbSet<IdentityDocument> IdentityDocuments => Set<IdentityDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up every IEntityTypeConfiguration<T> class in this assembly (reflection).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Ids are created in code (Guid v7), so tell EF the database never generates them.
        // Otherwise a new child added to a tracked person would be treated as an existing row.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType).Property(nameof(BaseEntity.Id)).ValueGeneratedNever();
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Store every enum as readable text ("Female") instead of a number.
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(32);

        // SQLite has no time-zone type; mark values read back as UTC so JSON gets the trailing 'Z'.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
        toDb => toDb.ToUniversalTime(),
        fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc));

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedAtUtc = now;
            if (entry.State == EntityState.Modified) entry.Entity.UpdatedAtUtc = now;
        }

        // Turn a hard delete into a soft delete.
        foreach (var entry in ChangeTracker.Entries<ISoftDeletable>().Where(e => e.State == EntityState.Deleted))
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAtUtc = now;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
