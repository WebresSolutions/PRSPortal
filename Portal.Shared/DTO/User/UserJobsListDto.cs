namespace Portal.Shared.DTO.User;

public class UserJobsListDto
{
    /// <summary>
    /// The user Id
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// The Collection of jobs for the user
    /// </summary>
    public UserJobDto[] UserJobs { get; set; } = [];
}
