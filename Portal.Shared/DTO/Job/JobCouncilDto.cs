namespace Portal.Shared.DTO.Job;

/// <summary>
/// Data transfer object representing council information for a job
/// Currently empty, reserved for future council-related properties
/// </summary>
public class JobCouncilDto
{
    public JobCouncilDto()
    {
    }
    public JobCouncilDto(int councilId, string councilName)
    {
        CouncilId = councilId;
        CouncilName = councilName;
    }

    public int CouncilId { get; set; }
    public string CouncilName { get; set; } = string.Empty;
}
