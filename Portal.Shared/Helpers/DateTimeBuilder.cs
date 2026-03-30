using System.Globalization;

namespace Portal.Shared.Helpers;

public static class DateTimeBuilder
{
    public static DateTime AsDateTime(DateOnly date, TimeOnly time) => new(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);

    public static string TimeRange(TimeOnly start, TimeOnly end) => $"{start:HH:mm}–{end:HH:mm}";

    public static string FormatHour(int h)
    {
        if (h == 12) return "12pm";
        if (h < 12) return $"{h}am";
        return $"{h - 12}pm";
    }

    public static string ToLocalDateTimeString(this DateTime dateTime) 
        => dateTime.ToLocalTime().ToString("g", CultureInfo.CreateSpecificCulture("en-AU"));

    public static string ToLocalDateOnlyString(this DateTime dateTime) 
        => dateTime.ToLocalTime().ToString("dd MM yyyy");
}
