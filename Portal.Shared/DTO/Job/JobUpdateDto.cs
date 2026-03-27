using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.User;
using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.DTO.Job;

public class JobUpdateDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the job.
    /// </summary>
    public int JobId { get; set; }
    /// <summary>
    /// The status of the jobs
    /// </summary>
    public int? JobStatusId { get; set; }
    [Required]
    public AddressDTO? Address { get; set; }
    /// <summary>
    /// Details about the job, such as scope, requirements, or any relevant information that provides context for the job.
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Details cannot exceed 2000 characters")]
    public string? Details { get; set; }
    /// <summary>
    /// Gets or sets the type of job to be processed.
    /// </summary>
    [Required(ErrorMessage = "Job type is required")]
    public JobTypeEnum[] JobTypes { get; set; } = [];
    /// <summary>
    /// Get or set the job colour ID
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Job colour ID must be at least 1 when provided")]
    public int? JobColourId { get; set; }
    /// <summary>
    /// The Contact Id
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Contact is required")]
    public int ContactId { get; set; }
    /// <summary>
    /// The Council Id
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Council ID must be positive when provided")]
    public int? CouncilId { get; set; }
    /// <summary>
    /// Holds the list of assigned users and their assignement type. 
    /// Responsible or Current Owner 
    /// </summary>
    [MinLength(1)]
    public List<UserAssignmentDto> AssignedUsers { get; set; } = [];
    /// <summary>
    /// Gets the details of a contact just to not reget them from the api
    /// </summary>
    public JobContactDto Contact { get; set; } = null!;
}
