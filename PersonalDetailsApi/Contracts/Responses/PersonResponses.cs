using PersonalDetailsApi.Domain.Enums;

namespace PersonalDetailsApi.Contracts.Responses;

// Positional records: immutable, value-based equality, concise syntax.

/// <summary>Lightweight row used in list results.</summary>
public sealed record PersonSummaryResponse(
    Guid Id,
    string FullName,
    string Email,
    string PrimaryPhone,
    int Age,
    Gender Gender,
    string Nationality,
    string? City);

/// <summary>A person with every related detail.</summary>
public sealed record PersonDetailResponse(
    Guid Id,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? PreferredName,
    string FullName,
    DateOnly DateOfBirth,
    int Age,
    string? PlaceOfBirth,
    Gender Gender,
    MaritalStatus MaritalStatus,
    BloodGroup BloodGroup,
    string Nationality,
    string Email,
    string PrimaryPhone,
    string? AlternatePhone,
    IReadOnlyList<string> Languages,
    string? Notes,
    IReadOnlyList<AddressResponse> Addresses,
    IReadOnlyList<EmergencyContactResponse> EmergencyContacts,
    IReadOnlyList<EducationResponse> Educations,
    IReadOnlyList<EmploymentResponse> Employments,
    IReadOnlyList<IdentityDocumentResponse> IdentityDocuments,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record AddressResponse(
    Guid Id, AddressType Type, string Line1, string? Line2, string City, string? State,
    string PostalCode, string Country, bool IsPrimary);

public sealed record EmergencyContactResponse(
    Guid Id, string Name, Relationship Relationship, string Phone, string? Email);

public sealed record EducationResponse(
    Guid Id, EducationLevel Level, string Institution, string? FieldOfStudy, int StartYear, int? EndYear, string? Grade);

public sealed record EmploymentResponse(
    Guid Id, string Employer, string JobTitle, EmploymentType EmploymentType,
    DateOnly StartDate, DateOnly? EndDate, bool IsCurrent);

/// <summary>Identity document. The number is masked (only the last 4 characters are shown).</summary>
public sealed record IdentityDocumentResponse(
    Guid Id, DocumentType DocumentType, string MaskedDocumentNumber, string IssuingCountry,
    DateOnly? IssueDate, DateOnly? ExpiryDate, bool IsExpired);

/// <summary>Aggregated figures across all stored persons.</summary>
public sealed record PersonStatisticsResponse(
    int TotalPersons,
    double AverageAge,
    IReadOnlyDictionary<string, int> ByGender,
    IReadOnlyDictionary<string, int> ByBloodGroup,
    IReadOnlyDictionary<string, int> ByNationality,
    IReadOnlyDictionary<string, int> ByAgeGroup);
