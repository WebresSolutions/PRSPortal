using Portal.Shared.DTO.User;

namespace Portal.Shared.DTO.Schedule;
/// <summary>
/// Used for getting a weekly schedule
/// </summary>
public class WeeklyScheduleDto
{
    public required int ScheduleTrackId { get; set; }
    public UserDto[] AssignedUsers { get; set; } = [];
    public required ScheduleDto Schedule { get; set; }
}
