using Portal.Shared.DTO.Schedule;

namespace Portal.Client.Webmodels;

/// <summary>
/// Extended schedule slot DTO with calendar event information
/// Adds calendar-specific properties for rendering in calendar views
/// </summary>
public class ScheduleTrackDtoWithCalendar : ScheduleTrackDto
{
    /// <summary>
    /// Gets or sets the list of calendar events for this schedule slot
    /// </summary>
    public List<CustomCalendarItem> Events { get; set; } = [];

    /// <summary>
    /// Populates the Events collection from the Schedule property
    /// Converts schedule items into calendar-compatible event objects
    /// </summary>
    public void SetEvents()
    {
        Events = [.. base.Schedule.Select(s => new CustomCalendarItem
        {
            Start =  s.StartAsDateTime(),
            End = s.EndAsDateTime(),
            Text = s.Description,
            Colour = s.Colour.ColourHex,
            JobNumber = s.Job?.JobNumber,
            JobAddress = s.Job?.Address is not null ? $"{s.Job.Address.Street}, {s.Job.Address.Suburb}, {s.Job.Address.PostCode}" : string.Empty,
            JobId = s.Job?.JobId,
            ColourId = s.Colour.ScheduleColourId,
            TrackId = s.ScheduleTrackId ?? throw new ArgumentNullException(nameof(s)),
            ScheduleItemId = s.ScheduleId
        })];
    }

    public ScheduleTrackDto GetDto() => new()
    {
        AssignedUsers = base.AssignedUsers,
        TrackId = base.TrackId,
        Day = base.Day,
        Schedule = base.Schedule,
    };
}
