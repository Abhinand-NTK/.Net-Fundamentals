using PersonalDetailsApi.Domain.Common;
using PersonalDetailsApi.Domain.Enums;

namespace PersonalDetailsApi.Domain.Entities;

/// <summary>
/// Aggregate root: a person and everything recorded about them.
/// Child collections (addresses, contacts, ...) are owned by and saved with the person.
/// </summary>
public class Person : AuditableEntity, ISoftDeletable
{
    // ---- Identity ----
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? PreferredName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? PlaceOfBirth { get; set; }
    public Gender Gender { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    public BloodGroup BloodGroup { get; set; }
    public string Nationality { get; set; } = string.Empty;

    // ---- Contact ----
    public string Email { get; set; } = string.Empty;
    public string PrimaryPhone { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }

    // ---- Extra ----
    /// <summary>Stored as a JSON column by EF Core (primitive collection).</summary>
    public List<string> Languages { get; set; } = [];
    public string? Notes { get; set; }

    // ---- Related data (one-to-many) ----
    public List<Address> Addresses { get; set; } = [];
    public List<EmergencyContact> EmergencyContacts { get; set; } = [];
    public List<Education> Educations { get; set; } = [];
    public List<Employment> Employments { get; set; } = [];
    public List<IdentityDocument> IdentityDocuments { get; set; } = [];

    // ---- Soft delete ----
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    /// <summary>Computed (not stored): joins the non-empty name parts.</summary>
    public string FullName =>
        string.Join(' ', new[] { FirstName, MiddleName, LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
}
