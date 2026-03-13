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
    public DateTime Start { get; set; }

    [Required]
    /// <summary>
    /// Gets or sets the end date and time of the schedule
    /// </summary>
    public DateTime End { get; set; }

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
    public string Description { get; set; } = string.Empty;

    public bool Delete { get; set; }
}
