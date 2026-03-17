using System.ComponentModel.DataAnnotations;

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
    [MaxLength(20)]
    public string ColourHex { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the description or name of the color
    /// </summary>
    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;
}
