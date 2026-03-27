using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Address;
using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.DTO.Job;

public class JobCreationDto
{
    /// <summary>
    /// The type of the job.
    /// </summary>
    [Required(ErrorMessage = "Job type is required")]
    public required List<JobTypeEnum> JobType { get; set; }

    /// <summary>
    /// Contact Id (required).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Contact is required")]
    public int ContactId { get; set; }

    /// <summary>
    /// The address details for the job. This is optional and can be null if not provided.
    /// </summary>
    public AddressDTO? Address { get; set; }

    /// <summary>
    /// The Council Id
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Council ID must be positive when provided")]
    public int? CouncilId { get; set; }

    /// <summary>
    /// Job Colour Id
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Job colour ID must be at least 1 when provided")]
    public int? JobColourId { get; set; }

    /// <summary>
    /// Description of the Job
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Details { get; set; }

    /// <summary>
    /// The status Id 
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Job status ID must be at least 1")]
    public int StatusId { get; set; }
}
