using System.ComponentModel.DataAnnotations;
using PersonalDetailsApi.Domain.Enums;

namespace PersonalDetailsApi.Contracts.Requests;

/// <summary>Query-string options for searching, filtering, sorting and paging the person list.</summary>
public sealed record PersonQueryParameters
{
    /// <summary>Matches first, middle, last or preferred name, or e-mail (case-insensitive, partial).</summary>
    [StringLength(100)]
    public string? Search { get; init; }

    public Gender? Gender { get; init; }
    public BloodGroup? BloodGroup { get; init; }
    public MaritalStatus? MaritalStatus { get; init; }

    /// <summary>Persons with at least one address in this city.</summary>
    [StringLength(100)]
    public string? City { get; init; }

    [Range(0, 150)]
    public int? MinAge { get; init; }

    [Range(0, 150)]
    public int? MaxAge { get; init; }

    public PersonSortField SortBy { get; init; } = PersonSortField.LastName;
    public SortDirection SortDirection { get; init; } = SortDirection.Asc;

    /// <summary>1-based page number.</summary>
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}
