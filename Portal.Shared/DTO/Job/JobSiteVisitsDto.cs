namespace Portal.Shared.DTO.Job;

/// <summary>
/// Data transfer object representing a job site visit
/// Contains scheduling information for site visits associated with a job
/// </summary>
public class JobSiteVisitsDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the schedule entry
    /// </summary>
    public int ScheduleId { get; set; }
    /// <summary>
    /// Gets or sets the start date and time of the site visit
    /// </summary>
    public DateTime Start { get; set; }
    /// <summary>
    /// Gets or sets the end date and time of the site visit
    /// </summary>
    public DateTime End { get; set; }
    /// <summary>
    /// Gets or sets the array of assignee names for the site visit
    /// </summary>
    public string[] Assignees { get; set; } = [];
    /// <summary>
    /// Gets or sets the category or type of the site visit
    /// </summary>
    public string Category { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets any notes or additional information about the site visit
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets a comma-separated string of assignee names
    /// </summary>
    /// <returns>A string containing all assignee names joined by commas</returns>
    public string GetAssigneesAsString()
    {
        return string.Join(", ", Assignees);
    }

    /// <summary>
    /// Gets a formatted string representation of the start and end times
    /// </summary>
    /// <returns>A string in the format "HH:mm - HH:mm"</returns>
    public string GetStartEndAsString()
    {
        return $"{Start:H:mm} - {End:H:mm}";
    }
}
