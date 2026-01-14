using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;

namespace Portal.Shared.DTO.Job;

/// <summary>
/// Data transfer object representing job information for list views
/// Contains essential job details for display in job listing pages
/// </summary>
public class ListJobDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the job
    /// </summary>
    public int JobId { get; set; }
    /// <summary>
    /// Gets or sets the address information for the job
    /// </summary>
    public AddressDTO Address { get; set; }
    /// <summary>
    /// Gets or sets the address identifier
    /// </summary>
    public int AddressId { get; set; }
    /// <summary>
    /// Gets or sets the primary contact information
    /// </summary>
    public ContactDto? Contact1 { get; set; }
    /// <summary>
    /// Gets or sets the secondary contact information
    /// </summary>
    public ContactDto? Contact2 { get; set; }
    /// <summary>
    /// Gets or sets the job number
    /// </summary>
    public int? JobNumber { get; set; }
    /// <summary>
    /// Gets or sets the job type as a string
    /// </summary>
    public string JobType { get; set; } = "";
    /// <summary>
    /// Gets or sets the job type as an integer identifier
    /// </summary>
    public int JobTypeInt { get; set; }
    /// <summary>
    /// Gets a formatted string representation of the job address
    /// </summary>
    public string AddressAsString => $"{Address.street}, {Address.suburb} {Address.State}";
    /// <summary>
    /// Gets a formatted string combining contact names
    /// Returns both contacts if available, otherwise returns the available contact or "No Contact"
    /// </summary>
    public string JoinedContactName => Contact1 != null && Contact2 != null
        ? $"{Contact1.fullName} / {Contact2.fullName}"
        : Contact1 != null
            ? Contact1.fullName
            : Contact2 != null
                ? Contact2.fullName
                : "No Contact";
    /// <summary>
    /// Initializes a new instance of the ListJobDto class
    /// </summary>
    /// <param name="jobId">The unique identifier for the job</param>
    /// <param name="address">The address information</param>
    /// <param name="contact1">The primary contact information</param>
    /// <param name="contact2">The secondary contact information</param>
    /// <param name="jobNumber">The job number</param>
    /// <param name="jobType">The job type as a string</param>
    /// <param name="jobTypeInt">The job type as an integer identifier</param>
    public ListJobDto(
        int jobId,
        AddressDTO address,
        ContactDto? contact1,
        ContactDto? contact2,
        int? jobNumber,
        string jobType,
        int jobTypeInt)
    {
        JobId = jobId;
        Address = address;
        Contact1 = contact1;
        Contact2 = contact2;
        JobNumber = jobNumber;
        JobType = jobType;
        JobTypeInt = jobTypeInt;
    }

    /// <summary>
    /// Parameterless constructor for serialization
    /// </summary>
    public ListJobDto()
    {
        Address = null!;
    }
}