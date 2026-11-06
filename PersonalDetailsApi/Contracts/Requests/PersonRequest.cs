using System.ComponentModel.DataAnnotations;
using PersonalDetailsApi.Common.Validation;
using PersonalDetailsApi.Domain.Enums;

namespace PersonalDetailsApi.Contracts.Requests;

/// <summary>
/// Payload for creating (POST) or fully replacing (PUT) a person, including all related details.
/// Validation uses data annotations plus <see cref="IValidatableObject"/> for cross-field rules.
/// </summary>
public sealed record PersonRequest : IValidatableObject
{
    /// <example>Aarav</example>
    [Required, StringLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [StringLength(100)]
    public string? MiddleName { get; init; }

    /// <example>Menon</example>
    [Required, StringLength(100)]
    public string LastName { get; init; } = string.Empty;

    [StringLength(100)]
    public string? PreferredName { get; init; }

    /// <summary>Date of birth (yyyy-MM-dd). Must be in the past and not more than 150 years ago.</summary>
    /// <example>1992-04-18</example>
    [Required, PastDate(MaxYearsAgo = 150)]
    public DateOnly DateOfBirth { get; init; }

    /// <example>Kochi</example>
    [StringLength(150)]
    public string? PlaceOfBirth { get; init; }

    [Required, EnumDataType(typeof(Gender))]
    public Gender Gender { get; init; }

    [EnumDataType(typeof(MaritalStatus))]
    public MaritalStatus MaritalStatus { get; init; } = MaritalStatus.Single;

    [EnumDataType(typeof(BloodGroup))]
    public BloodGroup BloodGroup { get; init; } = BloodGroup.Unknown;

    /// <example>Indian</example>
    [Required, StringLength(100)]
    public string Nationality { get; init; } = string.Empty;

    /// <example>aarav.menon@example.com</example>
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; init; } = string.Empty;

    /// <summary>Phone number in international format.</summary>
    /// <example>+91 98470 00001</example>
    [Required, PhoneNumber]
    public string PrimaryPhone { get; init; } = string.Empty;

    [PhoneNumber]
    public string? AlternatePhone { get; init; }

    /// <summary>Languages spoken.</summary>
    [MaxLength(20)]
    public List<string> Languages { get; init; } = [];

    [StringLength(2000)]
    public string? Notes { get; init; }

    [MaxLength(10)]
    public List<AddressRequest> Addresses { get; init; } = [];

    [MaxLength(10)]
    public List<EmergencyContactRequest> EmergencyContacts { get; init; } = [];

    [MaxLength(20)]
    public List<EducationRequest> Educations { get; init; } = [];

    [MaxLength(30)]
    public List<EmploymentRequest> Employments { get; init; } = [];

    [MaxLength(10)]
    public List<IdentityDocumentRequest> IdentityDocuments { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Addresses.Count(a => a.IsPrimary) > 1)
            yield return new ValidationResult("Only one address can be marked as primary.", [nameof(Addresses)]);

        if (Employments.Count(e => e.EndDate is null) > 3)
            yield return new ValidationResult("A person can have at most 3 current (open-ended) employments.", [nameof(Employments)]);

        var duplicateDocs = IdentityDocuments
            .GroupBy(d => (d.DocumentType, Number: d.DocumentNumber.Trim().ToUpperInvariant()))
            .Where(g => g.Count() > 1);
        if (duplicateDocs.Any())
            yield return new ValidationResult("The same identity document is listed more than once.", [nameof(IdentityDocuments)]);
    }
}

public sealed record AddressRequest
{
    [EnumDataType(typeof(AddressType))]
    public AddressType Type { get; init; } = AddressType.Home;

    /// <example>12 MG Road</example>
    [Required, StringLength(200)]
    public string Line1 { get; init; } = string.Empty;

    [StringLength(200)]
    public string? Line2 { get; init; }

    /// <example>Kochi</example>
    [Required, StringLength(100)]
    public string City { get; init; } = string.Empty;

    /// <example>Kerala</example>
    [StringLength(100)]
    public string? State { get; init; }

    /// <example>682016</example>
    [Required, StringLength(20)]
    public string PostalCode { get; init; } = string.Empty;

    /// <example>India</example>
    [Required, StringLength(100)]
    public string Country { get; init; } = string.Empty;

    public bool IsPrimary { get; init; }
}

public sealed record EmergencyContactRequest
{
    [Required, StringLength(150)]
    public string Name { get; init; } = string.Empty;

    [EnumDataType(typeof(Relationship))]
    public Relationship Relationship { get; init; }

    [Required, PhoneNumber]
    public string Phone { get; init; } = string.Empty;

    [EmailAddress, StringLength(256)]
    public string? Email { get; init; }
}

public sealed record EducationRequest : IValidatableObject
{
    [EnumDataType(typeof(EducationLevel))]
    public EducationLevel Level { get; init; }

    [Required, StringLength(200)]
    public string Institution { get; init; } = string.Empty;

    [StringLength(150)]
    public string? FieldOfStudy { get; init; }

    /// <example>2010</example>
    [Range(1900, 2100)]
    public int StartYear { get; init; }

    /// <example>2014</example>
    [Range(1900, 2100)]
    public int? EndYear { get; init; }

    [StringLength(20)]
    public string? Grade { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndYear < StartYear)
            yield return new ValidationResult("EndYear cannot be before StartYear.", [nameof(EndYear)]);
    }
}

public sealed record EmploymentRequest : IValidatableObject
{
    [Required, StringLength(200)]
    public string Employer { get; init; } = string.Empty;

    [Required, StringLength(150)]
    public string JobTitle { get; init; } = string.Empty;

    [EnumDataType(typeof(EmploymentType))]
    public EmploymentType EmploymentType { get; init; } = EmploymentType.FullTime;

    /// <example>2015-07-01</example>
    [Required]
    public DateOnly StartDate { get; init; }

    /// <summary>Leave empty for the current job.</summary>
    public DateOnly? EndDate { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate < StartDate)
            yield return new ValidationResult("EndDate cannot be before StartDate.", [nameof(EndDate)]);
    }
}

public sealed record IdentityDocumentRequest : IValidatableObject
{
    [EnumDataType(typeof(DocumentType))]
    public DocumentType DocumentType { get; init; }

    /// <example>P1234567</example>
    [Required, StringLength(50, MinimumLength = 3)]
    [RegularExpression("^[A-Za-z0-9-]+$", ErrorMessage = "DocumentNumber may contain only letters, digits and hyphens.")]
    public string DocumentNumber { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string IssuingCountry { get; init; } = string.Empty;

    public DateOnly? IssueDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IssueDate is { } issued && ExpiryDate is { } expires && expires <= issued)
            yield return new ValidationResult("ExpiryDate must be after IssueDate.", [nameof(ExpiryDate)]);
    }
}
