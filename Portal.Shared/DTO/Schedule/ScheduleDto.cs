using Portal.Shared.Helpers;

namespace Portal.Shared.DTO.Schedule;

/// <summary>
/// Data transfer object representing a schedule entry
/// Contains schedule timing, color, description, and associated job information
/// </summary>
public class ScheduleDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the schedule
    /// </summary>
    public int ScheduleId { get; set; }
    /// <summary>
    /// Gets or sets the start date and time of the schedule
    /// </summary>
    public TimeOnly Start { get; set; }
    /// <summary>
    /// Gets or sets the end date and time of the schedule
    /// </summary>
    public TimeOnly End { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the schedule slot this schedule belongs to
    /// </summary>
    public int? ScheduleTrackId { get; set; }

    public DateOnly ScheduleTrackDate { get; set; }
    /// <summary>
    /// Gets or sets the color information for the schedule
    /// </summary>
    public required ScheduleColourDto Colour { get; set; }
    /// <summary>
    /// Gets or sets the description or notes for the schedule
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the associated job information, if any
    /// </summary>
    public ScheduleJobPartialDto? Job { get; set; }

    public DateTime StartAsDateTime() => DateTimeBuilder.AsDateTime(this.ScheduleTrackDate, this.Start);
    public DateTime EndAsDateTime() => DateTimeBuilder.AsDateTime(this.ScheduleTrackDate, this.End);
}
