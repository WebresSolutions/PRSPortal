using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.DTO.Schedule;

public class UpdateScheduleDto
{
    public int Id { get; set; }

    [Required]
    public int TrackId { get; set; }

    [Required]
    /// <summary>
    /// Gets or sets the start date and time of the schedule
    /// </summary>
    public TimeOnly Start { get; set; }

    [Required]
    /// <summary>
    /// Gets or sets the end date and time of the schedule
    /// </summary>
    public TimeOnly End { get; set; }

    [Required]
    /// <summary>
    /// Gets or sets the color information for the schedule
    /// </summary>
    public int ColourId { get; set; }

    /// <summary>
    /// The optional Job Id
    /// </summary>
    public int? JobId { get; set; }

    /// <summary>
    /// Gets or sets the description or notes for the schedule
    /// </summary>
    [MaxLength(4000, ErrorMessage = "Notes cannot exceed 4000 characters")]
    public string Notes { get; set; } = string.Empty;

    public bool Delete { get; set; }
}
