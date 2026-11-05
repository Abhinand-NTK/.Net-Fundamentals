using PersonalDetailsApi.Domain.Common;
using PersonalDetailsApi.Domain.Enums;

namespace PersonalDetailsApi.Domain.Entities;

/// <summary>Base for every entity that belongs to a <see cref="Person"/>.</summary>
public abstract class PersonOwnedEntity : BaseEntity
{
    public Guid PersonId { get; set; }
    public Person? Person { get; set; }
}

public class Address : PersonOwnedEntity
{
    public AddressType Type { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class EmergencyContact : PersonOwnedEntity
{
    public string Name { get; set; } = string.Empty;
    public Relationship Relationship { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public class Education : PersonOwnedEntity
{
    public EducationLevel Level { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string? FieldOfStudy { get; set; }
    public int StartYear { get; set; }
    public int? EndYear { get; set; }
    public string? Grade { get; set; }
}

public class Employment : PersonOwnedEntity
{
    public string Employer { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public EmploymentType EmploymentType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    /// <summary>Expression-bodied computed property.</summary>
    public bool IsCurrent => EndDate is null;
}

public class IdentityDocument : PersonOwnedEntity
{
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string IssuingCountry { get; set; } = string.Empty;
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
}
