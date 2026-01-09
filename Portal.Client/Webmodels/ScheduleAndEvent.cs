using Heron.MudCalendar;
using Portal.Shared.DTO.Schedule;

namespace Portal.Client.Webmodels;

/// <summary>
/// Extended schedule slot DTO with calendar event information
/// Adds calendar-specific properties for rendering in calendar views
/// </summary>
public class ScheduleSlotDtoWithCalendar : ScheduleSlotDTO
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
            Start = s.Start,
            End = s.End,
            Text = s.Description,
            Colour = s.Colour.ColourHex,
            JobNumber = s.Job?.JobNumber,
            JobAddress = s.Job?.Address is not null ? $"{s.Job.Address.street}, {s.Job.Address.suburb}, {s.Job.Address.postCode}" : string.Empty
        })];
    }
}

/// <summary>
/// Custom calendar item with additional job-related properties
/// Extends the base calendar item with job number and address information
/// </summary>
public class CustomCalendarItem : CalendarItem
{
    /// <summary>
    /// Gets or sets the color hex code for the calendar item
    /// </summary>
    public required string Colour { get; set; }
    /// <summary>
    /// Gets or sets the job number associated with this calendar item
    /// </summary>
    public int? JobNumber { get; set; }
    /// <summary>
    /// Gets or sets the formatted job address string
    /// </summary>
    public string JobAddress { get; set; } = string.Empty;
}