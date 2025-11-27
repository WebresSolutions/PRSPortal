using Portal.Shared.DTO.Address;

namespace Portal.Shared.DTO.Job;

/// <summary>
/// 
/// </summary>
public class ListJobDto
{
    public int JobId { get; set; }
    public AddressDTO Address { get; set; }
    public int AddressId { get; set; }
    public int ContactId { get; set; }
    public string Contact { get; set; }
    public int? JobNumber { get; set; }
    public string JobType { get; set; }
    public string AddressAsString => $"{Address.street}, {Address.suburb} {Address.State}";

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
        int addressId,
        int contactId,
        string contact,
        int? jobNumber,
        string jobType)
    {
        JobId = jobId;
        Address = address;
        AddressId = addressId;
        ContactId = contactId;
        Contact = contact;
        JobNumber = jobNumber;
        JobType = jobType;
    }

    /// <summary>
    /// Parameterless constructor for serialization
    /// </summary>
    public ListJobDto()
    {
        Contact = string.Empty;
        Address = null!;
    }
}