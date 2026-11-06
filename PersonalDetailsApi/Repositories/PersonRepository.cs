using Microsoft.EntityFrameworkCore;
using PersonalDetailsApi.Common.Extensions;
using PersonalDetailsApi.Contracts.Requests;
using PersonalDetailsApi.Contracts.Responses;
using PersonalDetailsApi.Data;
using PersonalDetailsApi.Domain.Entities;
using PersonalDetailsApi.Domain.Enums;

namespace PersonalDetailsApi.Repositories;

/// <summary>Slim projection used for statistics, so we don't load whole entities.</summary>
public sealed record PersonStatRow(Gender Gender, BloodGroup BloodGroup, string Nationality, DateOnly DateOfBirth);

public interface IPersonRepository : IRepository<Person>
{
    /// <summary>Loads a person with all related collections.</summary>
    Task<Person?> GetWithDetailsAsync(Guid id, bool track, CancellationToken ct = default);

    Task<PagedResult<Person>> SearchAsync(PersonQueryParameters query, DateOnly today, CancellationToken ct = default);

    Task<IReadOnlyList<PersonStatRow>> GetStatRowsAsync(CancellationToken ct = default);
}

public sealed class PersonRepository(AppDbContext db) : Repository<Person>(db), IPersonRepository
{
    public async Task<Person?> GetWithDetailsAsync(Guid id, bool track, CancellationToken ct = default)
    {
        IQueryable<Person> query = Set
            .Include(p => p.Addresses)
            .Include(p => p.EmergencyContacts)
            .Include(p => p.Educations)
            .Include(p => p.Employments)
            .Include(p => p.IdentityDocuments)
            .AsSplitQuery(); // one SQL query per collection avoids a cartesian explosion

        if (!track) query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public Task<PagedResult<Person>> SearchAsync(PersonQueryParameters q, DateOnly today, CancellationToken ct = default)
    {
        IQueryable<Person> query = Set.AsNoTracking().Include(p => p.Addresses);

        // ---- Filtering (each 'if' adds a WHERE clause; nothing runs until the query is enumerated) ----
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var pattern = $"%{q.Search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.FirstName, pattern) ||
                EF.Functions.Like(p.LastName, pattern) ||
                (p.MiddleName != null && EF.Functions.Like(p.MiddleName, pattern)) ||
                (p.PreferredName != null && EF.Functions.Like(p.PreferredName, pattern)) ||
                EF.Functions.Like(p.Email, pattern));
        }

        if (q.Gender is { } gender) query = query.Where(p => p.Gender == gender);
        if (q.BloodGroup is { } blood) query = query.Where(p => p.BloodGroup == blood);
        if (q.MaritalStatus is { } marital) query = query.Where(p => p.MaritalStatus == marital);

        if (!string.IsNullOrWhiteSpace(q.City))
        {
            var city = q.City.Trim();
            query = query.Where(p => p.Addresses.Any(a => EF.Functions.Like(a.City, city)));
        }

        // Age filters are translated to date-of-birth ranges so they can run in SQL.
        if (q.MinAge is { } minAge)
        {
            var latestDob = today.AddYears(-minAge);
            query = query.Where(p => p.DateOfBirth <= latestDob);
        }
        if (q.MaxAge is { } maxAge)
        {
            var earliestDob = today.AddYears(-(maxAge + 1)).AddDays(1);
            query = query.Where(p => p.DateOfBirth >= earliestDob);
        }

        // ---- Sorting (switch expression) with a stable tie-breaker on Id ----
        var sorted = q.SortBy switch
        {
            PersonSortField.FirstName => query.OrderBy(p => p.FirstName, q.SortDirection).ThenBy(p => p.LastName, q.SortDirection),
            PersonSortField.DateOfBirth => query.OrderBy(p => p.DateOfBirth, q.SortDirection),
            PersonSortField.CreatedAt => query.OrderBy(p => p.CreatedAtUtc, q.SortDirection),
            _ => query.OrderBy(p => p.LastName, q.SortDirection).ThenBy(p => p.FirstName, q.SortDirection),
        };

        return sorted.ThenBy(p => p.Id).ToPagedResultAsync(q.Page, q.PageSize, ct);
    }

    public async Task<IReadOnlyList<PersonStatRow>> GetStatRowsAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking()
            .Select(p => new PersonStatRow(p.Gender, p.BloodGroup, p.Nationality, p.DateOfBirth))
            .ToListAsync(ct);
}
