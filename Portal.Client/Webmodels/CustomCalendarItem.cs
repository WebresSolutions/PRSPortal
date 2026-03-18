namespace Portal.Client.Webmodels;

/// <summary>
/// Calendar item with job-related properties for schedule day view (7am–6pm).
/// </summary>
public class CustomCalendarItem
{
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool AllDay { get; set; }

    public int ScheduleItemId { get; set; }
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
    /// <summary>
    /// Gets or sets the unique identifier of the job associated with this entity.
    /// </summary>
    public int? JobId { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier for the colour.
    /// </summary>
    public int ColourId { get; set; }
    /// <summary>
    /// The track Id
    /// </summary>
    public int TrackId { get; set; }
}