namespace PersonalDetailsApi.Common.Extensions;

/// <summary>Extension methods: add behaviour to an existing type without modifying it.</summary>
public static class DateExtensions
{
    /// <summary>Age in whole years on <paramref name="today"/>.</summary>
    public static int AgeOn(this DateOnly dateOfBirth, DateOnly today)
    {
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--; // birthday not reached yet this year
        return age;
    }

    public static DateOnly Today(this TimeProvider timeProvider) =>
        DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
}

public static class StringExtensions
{
    /// <summary>Masks all but the last <paramref name="visible"/> characters, e.g. "P1234567" → "****4567".</summary>
    public static string Mask(this string value, int visible = 4) =>
        value.Length <= visible ? new string('*', value.Length) : new string('*', value.Length - visible) + value[^visible..];

    /// <summary>Trims and converts empty strings to null.</summary>
    public static string? NullIfWhiteSpace(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
