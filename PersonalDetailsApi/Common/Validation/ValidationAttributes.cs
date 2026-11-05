using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PersonalDetailsApi.Common.Validation;

/// <summary>Custom attribute: the date must be in the past, and optionally within a number of years.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class PastDateAttribute : ValidationAttribute
{
    public int MaxYearsAgo { get; set; } = 0; // 0 = no lower bound

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not DateOnly date) return ValidationResult.Success; // [Required] handles nulls

        var timeProvider = context.GetService(typeof(TimeProvider)) as TimeProvider ?? TimeProvider.System;
        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        string[] members = context.MemberName is null ? [] : [context.MemberName];

        if (date >= today)
            return new ValidationResult($"{context.DisplayName} must be in the past.", members);

        if (MaxYearsAgo > 0 && date < today.AddYears(-MaxYearsAgo))
            return new ValidationResult($"{context.DisplayName} cannot be more than {MaxYearsAgo} years ago.", members);

        return ValidationResult.Success;
    }
}

/// <summary>
/// International phone number: optional leading '+', then 7-15 digits; spaces, hyphens and brackets allowed.
/// Uses a source-generated regex (compiled at build time).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed partial class PhoneNumberAttribute : ValidationAttribute
{
    public PhoneNumberAttribute() : base("{0} must be a valid phone number, e.g. +91 98470 00001.") { }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is not string s || !AllowedCharacters().IsMatch(s)) return false;

        var digits = s.Count(char.IsDigit);
        return digits is >= 7 and <= 15; // relational pattern matching
    }

    [GeneratedRegex(@"^\+?[0-9\s\-()]+$")]
    private static partial Regex AllowedCharacters();
}
