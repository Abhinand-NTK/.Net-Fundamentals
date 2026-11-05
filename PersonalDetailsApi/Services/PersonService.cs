using PersonalDetailsApi.Common.Exceptions;
using PersonalDetailsApi.Common.Extensions;
using PersonalDetailsApi.Contracts.Requests;
using PersonalDetailsApi.Contracts.Responses;
using PersonalDetailsApi.Domain.Entities;
using PersonalDetailsApi.Mapping;
using PersonalDetailsApi.Repositories;

namespace PersonalDetailsApi.Services;

/// <summary>Business operations on persons. Controllers depend on this interface, not on EF Core.</summary>
public interface IPersonService
{
    Task<PagedResult<PersonSummaryResponse>> SearchAsync(PersonQueryParameters query, CancellationToken ct = default);
    Task<PersonDetailResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PersonDetailResponse> CreateAsync(PersonRequest request, CancellationToken ct = default);
    Task<PersonDetailResponse> UpdateAsync(Guid id, PersonRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<PersonStatisticsResponse> GetStatisticsAsync(CancellationToken ct = default);
}

/// <summary>
/// Default implementation. Dependencies arrive through the primary constructor (constructor injection).
/// </summary>
public sealed class PersonService(
    IPersonRepository repository,
    TimeProvider timeProvider,
    ILogger<PersonService> logger) : IPersonService
{
    private DateOnly Today => timeProvider.Today();

    public async Task<PagedResult<PersonSummaryResponse>> SearchAsync(PersonQueryParameters query, CancellationToken ct = default)
    {
        var page = await repository.SearchAsync(query, Today, ct);
        var today = Today;
        return page.Map(p => p.ToSummary(today));
    }

    public async Task<PersonDetailResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var person = await repository.GetWithDetailsAsync(id, track: false, ct)
                     ?? throw new NotFoundException(nameof(Person), id);
        return person.ToDetail(Today);
    }

    public async Task<PersonDetailResponse> CreateAsync(PersonRequest request, CancellationToken ct = default)
    {
        await EnsureEmailIsUniqueAsync(request.Email, excludeId: null, ct);

        var person = request.ToEntity();
        repository.Add(person);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Created person {PersonId}", person.Id);
        return person.ToDetail(Today);
    }

    public async Task<PersonDetailResponse> UpdateAsync(Guid id, PersonRequest request, CancellationToken ct = default)
    {
        var person = await repository.GetWithDetailsAsync(id, track: true, ct)
                     ?? throw new NotFoundException(nameof(Person), id);

        await EnsureEmailIsUniqueAsync(request.Email, excludeId: id, ct);

        request.ApplyTo(person);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Updated person {PersonId}", id);
        return person.ToDetail(Today);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var person = await repository.GetByIdAsync(id, ct)
                     ?? throw new NotFoundException(nameof(Person), id);

        repository.Remove(person); // AppDbContext converts this into a soft delete
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Soft-deleted person {PersonId}", id);
    }

    public async Task<PersonStatisticsResponse> GetStatisticsAsync(CancellationToken ct = default)
    {
        var rows = await repository.GetStatRowsAsync(ct);
        var today = Today;
        var ages = rows.Select(r => r.DateOfBirth.AgeOn(today)).ToList();

        // LINQ-to-objects: GroupBy + ToDictionary, ordered by count.
        static Dictionary<string, int> CountBy<TKey>(IEnumerable<TKey> keys) where TKey : notnull =>
            keys.GroupBy(k => k)
                .OrderByDescending(g => g.Count())
                .ToDictionary(g => g.Key.ToString()!, g => g.Count());

        return new PersonStatisticsResponse(
            TotalPersons: rows.Count,
            AverageAge: ages.Count == 0 ? 0 : Math.Round(ages.Average(), 1),
            ByGender: CountBy(rows.Select(r => r.Gender)),
            ByBloodGroup: CountBy(rows.Select(r => r.BloodGroup)),
            ByNationality: CountBy(rows.Select(r => r.Nationality)),
            ByAgeGroup: CountBy(ages.Select(ToAgeGroup)));
    }

    private static string ToAgeGroup(int age) => age switch
    {
        < 18 => "Under 18",
        < 30 => "18-29",
        < 45 => "30-44",
        < 60 => "45-59",
        _ => "60+",
    };

    private async Task EnsureEmailIsUniqueAsync(string email, Guid? excludeId, CancellationToken ct)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var taken = await repository.ExistsAsync(p => p.Email == normalized && p.Id != excludeId, ct);
        if (taken) throw new ConflictException($"A person with e-mail '{normalized}' already exists.");
    }
}
