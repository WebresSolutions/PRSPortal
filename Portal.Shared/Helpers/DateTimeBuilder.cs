namespace Portal.Shared.Helpers;

public static class DateTimeBuilder
{
    public static DateTime AsDateTime(DateOnly date, TimeOnly time) => new(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);

    public static string TimeRange(TimeOnly start, TimeOnly end) => $"{start:HH:mm}–{end:HH:mm}";
}
