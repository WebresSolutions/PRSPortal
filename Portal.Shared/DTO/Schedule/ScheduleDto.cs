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
    public DateTime Start { get; set; }
    /// <summary>
    /// Gets or sets the end date and time of the schedule
    /// </summary>
    public DateTime End { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the schedule slot this schedule belongs to
    /// </summary>
    public int? ScheduleSlotID { get; set; }
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
}
