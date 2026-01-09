namespace Portal.Shared.DTO.Job;

public class JobSiteVisitsDto
{
    public int ScheduleId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string[] Assignees { get; set; } = [];
    public string Category { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string GetAssigneesAsString()
    {
        return string.Join(", ", Assignees);
    }

    public string GetStartEndAsString()
    {
        return $"{Start:H:mm} - {End:H:mm}";
    }
}
