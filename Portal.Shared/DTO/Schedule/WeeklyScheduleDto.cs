using Portal.Shared.DTO.User;
using Portal.Shared.Helpers;

namespace Portal.Shared.DTO.Schedule;
/// <summary>
/// Used for getting a weekly schedule
/// </summary>
public class WeeklyScheduleDto
{
    public required int ScheduleTrackId { get; set; }
    public required DateOnly TrackDate { get; set; }
    public UserDto[] AssignedUsers { get; set; } = [];
    public required ScheduleDto Schedule { get; set; }

    public DateTime StartAsDateTime() => DateTimeBuilder.AsDateTime(this.TrackDate, this.Schedule.Start);
    public DateTime EndAsDateTime() => DateTimeBuilder.AsDateTime(this.TrackDate, this.Schedule.End);
}
