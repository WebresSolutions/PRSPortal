namespace Portal.Shared.DTO.Schedule;

public class WeeklyGroupedByScheduleDto
{
    public DateOnly Date { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public WeeklyScheduleDto[] Schedules { get; set; } = [];
}
