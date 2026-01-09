namespace Portal.Shared.DTO.Schedule;

/// <summary>
/// Data transfer object representing a schedule color
/// Contains color information for schedule display
/// </summary>
public class ScheduleColourDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the schedule color
    /// </summary>
    public int ScheduleColourId { get; set; }
    /// <summary>
    /// Gets or sets the hexadecimal color code (e.g., "#FF0000")
    /// </summary>
    public string ColourHex { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the description or name of the color
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
