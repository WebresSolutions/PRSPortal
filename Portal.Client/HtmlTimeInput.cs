using System.Globalization;

namespace Portal.Client;

/// <summary>
/// Maps time-of-day values to HTML <c>input type="time"</c> strings (24h <c>HH:mm</c>).
/// </summary>
public static class HtmlTimeInput
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static string Format(TimeSpan timeOfDay) =>
        TimeOnly.FromTimeSpan(timeOfDay).ToString("HH:mm", Invariant);

    public static string FormatNullable(TimeSpan? timeOfDay) =>
        timeOfDay is { } t ? Format(t) : "";

    public static bool TryParseTimeOfDay(string? value, out TimeSpan timeOfDay)
    {
        timeOfDay = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;
        if (!TimeOnly.TryParse(value, Invariant, DateTimeStyles.None, out TimeOnly t))
            return false;
        timeOfDay = t.ToTimeSpan();
        return true;
    }
}
