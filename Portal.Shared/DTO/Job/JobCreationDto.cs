using Portal.Shared.DTO.Address;

namespace Portal.Shared.DTO.Job;

public class JobCreationDto
{
    /// <summary>
    /// The job number.
    /// </summary>
    public required int JobNumber { get; set; }

    /// <summary>
    /// The type of the job.
    /// </summary>
    public required JobTypeEnum JobType { get; set; }

    /// <summary>
    /// Optional Contact Id
    /// </summary>
    public int ContactId { get; set; }

    /// <summary>
    /// The address details for the job. This is optional and can be null if not provided.
    /// </summary>
    public AddressDTO? Address { get; set; }

    /// <summary>
    /// The Council Id
    /// </summary>
    public int? CouncilId { get; set; }

    /// <summary>
    /// Job Colour Id
    /// </summary>
    public int? JobColourId { get; set; }

    /// <summary>
    /// Description of the Job 
    /// </summary>
    public string? Description { get; set; }

}
