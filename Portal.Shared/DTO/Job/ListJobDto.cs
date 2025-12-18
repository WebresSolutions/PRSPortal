using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;

namespace Portal.Shared.DTO.Job;

/// <summary>
/// 
/// </summary>
public class ListJobDto
{
    public int JobId { get; set; }
    public AddressDTO Address { get; set; }
    public int AddressId { get; set; }
    public ContactDto? Contact1 { get; set; }
    public ContactDto? Contact2 { get; set; }
    public int? JobNumber { get; set; }
    public string JobType { get; set; } = "";
    public int JobTypeInt { get; set; }
    public string AddressAsString => $"{Address.street}, {Address.suburb} {Address.State}";
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
    /// <param name="jobId"></param>
    /// <param name="address"></param>
    /// <param name="addressId"></param>
    /// <param name="contactId"></param>
    /// <param name="contact"></param>
    /// <param name="setoutNumber"></param>
    /// <param name="cadNumber"></param>
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