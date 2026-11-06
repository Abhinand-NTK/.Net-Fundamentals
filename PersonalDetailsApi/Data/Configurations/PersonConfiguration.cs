using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalDetailsApi.Domain.Entities;

namespace PersonalDetailsApi.Data.Configurations;

/// <summary>Fluent API mapping for <see cref="Person"/> and its relationships.</summary>
public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Persons");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.MiddleName).HasMaxLength(100);
        builder.Property(p => p.LastName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.PreferredName).HasMaxLength(100);
        builder.Property(p => p.PlaceOfBirth).HasMaxLength(150);
        builder.Property(p => p.Nationality).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Email).HasMaxLength(256).IsRequired();
        builder.Property(p => p.PrimaryPhone).HasMaxLength(30).IsRequired();
        builder.Property(p => p.AlternatePhone).HasMaxLength(30);
        builder.Property(p => p.Notes).HasMaxLength(2000);

        builder.Ignore(p => p.FullName); // computed, not a column

        // Unique e-mail among non-deleted persons (filtered index).
        builder.HasIndex(p => p.Email).IsUnique().HasFilter("IsDeleted = 0");
        builder.HasIndex(p => new { p.LastName, p.FirstName });

        // Global query filter: soft-deleted rows are invisible to every query by default.
        builder.HasQueryFilter(p => !p.IsDeleted);

        // One-to-many relationships; children are removed together with the person.
        builder.HasMany(p => p.Addresses).WithOne(a => a.Person).HasForeignKey(a => a.PersonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.EmergencyContacts).WithOne(c => c.Person).HasForeignKey(c => c.PersonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Educations).WithOne(e => e.Person).HasForeignKey(e => e.PersonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Employments).WithOne(e => e.Person).HasForeignKey(e => e.PersonId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.IdentityDocuments).WithOne(d => d.Person).HasForeignKey(d => d.PersonId).OnDelete(DeleteBehavior.Cascade);
    }
}
