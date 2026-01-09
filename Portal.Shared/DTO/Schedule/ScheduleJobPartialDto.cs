using Portal.Shared.DTO.Address;

namespace Portal.Shared.DTO.Schedule;

/// <summary>
/// Data transfer object representing partial job information for schedules
/// Contains minimal job details needed for schedule display
/// </summary>
public class ScheduleJobPartialDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the job
    /// </summary>
    public int JobId { get; set; }
    /// <summary>
    /// Gets or sets the address information for the job
    /// </summary>
    public AddressDTO? Address { get; set; }
    /// <summary>
    /// Gets or sets the job number
    /// </summary>
    public int? JobNumber { get; set; }
}
