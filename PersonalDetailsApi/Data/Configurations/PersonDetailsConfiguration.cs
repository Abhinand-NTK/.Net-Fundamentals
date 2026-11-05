using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalDetailsApi.Domain.Entities;

namespace PersonalDetailsApi.Data.Configurations;

/// <summary>
/// Shared configuration for every child of <see cref="Person"/> (template method pattern):
/// the base class applies the common rule, subclasses add their own column mappings.
/// </summary>
public abstract class PersonOwnedConfiguration<T> : IEntityTypeConfiguration<T> where T : PersonOwnedEntity
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
        // Hide children of soft-deleted persons, matching the Person query filter.
        builder.HasQueryFilter(e => !e.Person!.IsDeleted);
        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);
}

public class AddressConfiguration : PersonOwnedConfiguration<Address>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Address> builder)
    {
        builder.Property(a => a.Line1).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Line2).HasMaxLength(200);
        builder.Property(a => a.City).HasMaxLength(100).IsRequired();
        builder.Property(a => a.State).HasMaxLength(100);
        builder.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Country).HasMaxLength(100).IsRequired();
        builder.HasIndex(a => a.City);
    }
}

public class EmergencyContactConfiguration : PersonOwnedConfiguration<EmergencyContact>
{
    protected override void ConfigureEntity(EntityTypeBuilder<EmergencyContact> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Phone).HasMaxLength(30).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(256);
    }
}

public class EducationConfiguration : PersonOwnedConfiguration<Education>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Education> builder)
    {
        builder.Property(e => e.Institution).HasMaxLength(200).IsRequired();
        builder.Property(e => e.FieldOfStudy).HasMaxLength(150);
        builder.Property(e => e.Grade).HasMaxLength(20);
    }
}

public class EmploymentConfiguration : PersonOwnedConfiguration<Employment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Employment> builder)
    {
        builder.Property(e => e.Employer).HasMaxLength(200).IsRequired();
        builder.Property(e => e.JobTitle).HasMaxLength(150).IsRequired();
        builder.Ignore(e => e.IsCurrent);
    }
}

public class IdentityDocumentConfiguration : PersonOwnedConfiguration<IdentityDocument>
{
    protected override void ConfigureEntity(EntityTypeBuilder<IdentityDocument> builder)
    {
        builder.Property(d => d.DocumentNumber).HasMaxLength(50).IsRequired();
        builder.Property(d => d.IssuingCountry).HasMaxLength(100).IsRequired();
    }
}
