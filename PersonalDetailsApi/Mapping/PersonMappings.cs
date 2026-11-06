using PersonalDetailsApi.Common.Extensions;
using PersonalDetailsApi.Contracts.Requests;
using PersonalDetailsApi.Contracts.Responses;
using PersonalDetailsApi.Domain.Entities;

namespace PersonalDetailsApi.Mapping;

/// <summary>
/// Hand-written mapping between API contracts and domain entities, using extension methods.
/// Explicit mapping keeps the API shape independent of the database shape.
/// </summary>
public static class PersonMappings
{
    // ---------- Request -> Entity ----------

    public static Person ToEntity(this PersonRequest request)
    {
        var person = new Person();
        request.ApplyTo(person);
        return person;
    }

    /// <summary>Copies every value from the request onto the entity; child collections are replaced.</summary>
    public static void ApplyTo(this PersonRequest r, Person p)
    {
        p.FirstName = r.FirstName.Trim();
        p.MiddleName = r.MiddleName.NullIfWhiteSpace();
        p.LastName = r.LastName.Trim();
        p.PreferredName = r.PreferredName.NullIfWhiteSpace();
        p.DateOfBirth = r.DateOfBirth;
        p.PlaceOfBirth = r.PlaceOfBirth.NullIfWhiteSpace();
        p.Gender = r.Gender;
        p.MaritalStatus = r.MaritalStatus;
        p.BloodGroup = r.BloodGroup;
        p.Nationality = r.Nationality.Trim();
        p.Email = r.Email.Trim().ToLowerInvariant();
        p.PrimaryPhone = r.PrimaryPhone.Trim();
        p.AlternatePhone = r.AlternatePhone.NullIfWhiteSpace();
        p.Languages = r.Languages.Select(l => l.Trim()).Where(l => l.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        p.Notes = r.Notes.NullIfWhiteSpace();

        Replace(p.Addresses, r.Addresses, a => new Address
        {
            Type = a.Type, Line1 = a.Line1.Trim(), Line2 = a.Line2.NullIfWhiteSpace(), City = a.City.Trim(),
            State = a.State.NullIfWhiteSpace(), PostalCode = a.PostalCode.Trim(), Country = a.Country.Trim(),
            // If no address is flagged primary, the first one becomes primary.
            IsPrimary = a.IsPrimary || (!r.Addresses.Any(x => x.IsPrimary) && ReferenceEquals(a, r.Addresses[0])),
        });
        Replace(p.EmergencyContacts, r.EmergencyContacts, c => new EmergencyContact
        {
            Name = c.Name.Trim(), Relationship = c.Relationship, Phone = c.Phone.Trim(), Email = c.Email.NullIfWhiteSpace(),
        });
        Replace(p.Educations, r.Educations, e => new Education
        {
            Level = e.Level, Institution = e.Institution.Trim(), FieldOfStudy = e.FieldOfStudy.NullIfWhiteSpace(),
            StartYear = e.StartYear, EndYear = e.EndYear, Grade = e.Grade.NullIfWhiteSpace(),
        });
        Replace(p.Employments, r.Employments, e => new Employment
        {
            Employer = e.Employer.Trim(), JobTitle = e.JobTitle.Trim(), EmploymentType = e.EmploymentType,
            StartDate = e.StartDate, EndDate = e.EndDate,
        });
        Replace(p.IdentityDocuments, r.IdentityDocuments, d => new IdentityDocument
        {
            DocumentType = d.DocumentType, DocumentNumber = d.DocumentNumber.Trim().ToUpperInvariant(),
            IssuingCountry = d.IssuingCountry.Trim(), IssueDate = d.IssueDate, ExpiryDate = d.ExpiryDate,
        });
    }

    /// <summary>Generic helper: clears a collection and refills it from mapped source items.</summary>
    private static void Replace<TEntity, TSource>(List<TEntity> target, IEnumerable<TSource> source, Func<TSource, TEntity> map)
    {
        target.Clear();
        target.AddRange(source.Select(map));
    }

    // ---------- Entity -> Response ----------

    public static PersonSummaryResponse ToSummary(this Person p, DateOnly today) => new(
        p.Id, p.FullName, p.Email, p.PrimaryPhone, p.DateOfBirth.AgeOn(today), p.Gender, p.Nationality,
        (p.Addresses.FirstOrDefault(a => a.IsPrimary) ?? p.Addresses.FirstOrDefault())?.City);

    public static PersonDetailResponse ToDetail(this Person p, DateOnly today) => new(
        p.Id, p.FirstName, p.MiddleName, p.LastName, p.PreferredName, p.FullName,
        p.DateOfBirth, p.DateOfBirth.AgeOn(today), p.PlaceOfBirth,
        p.Gender, p.MaritalStatus, p.BloodGroup, p.Nationality,
        p.Email, p.PrimaryPhone, p.AlternatePhone, p.Languages, p.Notes,
        p.Addresses.OrderByDescending(a => a.IsPrimary)
            .Select(a => new AddressResponse(a.Id, a.Type, a.Line1, a.Line2, a.City, a.State, a.PostalCode, a.Country, a.IsPrimary))
            .ToList(),
        p.EmergencyContacts
            .Select(c => new EmergencyContactResponse(c.Id, c.Name, c.Relationship, c.Phone, c.Email))
            .ToList(),
        p.Educations.OrderByDescending(e => e.StartYear)
            .Select(e => new EducationResponse(e.Id, e.Level, e.Institution, e.FieldOfStudy, e.StartYear, e.EndYear, e.Grade))
            .ToList(),
        p.Employments.OrderByDescending(e => e.StartDate)
            .Select(e => new EmploymentResponse(e.Id, e.Employer, e.JobTitle, e.EmploymentType, e.StartDate, e.EndDate, e.IsCurrent))
            .ToList(),
        p.IdentityDocuments
            .Select(d => new IdentityDocumentResponse(d.Id, d.DocumentType, d.DocumentNumber.Mask(), d.IssuingCountry,
                d.IssueDate, d.ExpiryDate, d.ExpiryDate is { } exp && exp < today))
            .ToList(),
        p.CreatedAtUtc, p.UpdatedAtUtc);
}
