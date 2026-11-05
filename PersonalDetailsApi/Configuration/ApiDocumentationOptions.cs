using System.ComponentModel.DataAnnotations;

namespace PersonalDetailsApi.Configuration;

/// <summary>
/// Strongly-typed settings bound from the "ApiDocumentation" section of appsettings.json (options pattern).
/// </summary>
public sealed class ApiDocumentationOptions
{
    public const string SectionName = "ApiDocumentation";

    [Required] public string Title { get; init; } = "Personal Details API";
    [Required] public string Version { get; init; } = "v1";
    public string Description { get; init; } = string.Empty;
    public string? ContactName { get; init; }
    public string? ContactUrl { get; init; }

    /// <summary>Serve Swagger UI. Usually on in Development only.</summary>
    public bool Enabled { get; init; } = true;
}
