using Portal.Shared.DTO.Address;
using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.DTO.Job;

public class JobDetailsDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the job.
    /// </summary>
    public int JobId { get; set; }
    /// <summary>
    /// Gets or sets the job number associated with this instance.
    /// </summary>
    public int JobNumber { get; set; }
    /// <summary>
    /// Gets or sets the type of job to be processed.
    /// </summary>
    public JobTypeEnum JobType { get; set; }
    /// <summary>
    /// Gets or sets the address information associated with this entity.
    /// </summary>
    [Required]
    public required AddressDTO Address { get; set; }
    /// <summary>
    /// Gets or sets the colour information associated with the job.
    /// </summary>
    public JobColourDto? Colour { get; set; }
    /// <summary>
    /// Gets or sets the collection of notes associated with the job.
    /// </summary>
    public List<JobNoteDto> Notes { get; set; } = [];
}
