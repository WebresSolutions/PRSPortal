using Portal.Shared.DTO.Job;
using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.DTO.User;

public class UserJobDto
{
    public UserJobDto(int userId, int jobId, ListJobDto job)
    {
        this.JobId = jobId;
        this.UserId = userId;
        this.JobDetails = job;
    }

    public UserJobDto()
    {
        JobDetails = new();
    }

    public int UserId { get; set; }
    public int JobId { get; set; }
    [Required]
    public ListJobDto JobDetails { get; set; }
}
