using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Job;

namespace Portal.Shared.DTO.Contact;

/// <summary>
/// Data transfer object representing detailed contact information
/// Contains complete contact details including address and related jobs
/// </summary>
public record ContactDetailsDto(
    int ContactId,
    string FullName,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Fax,
    AddressDTO? Address,
    ContactDto? ParentContact,
    string CreatedBy,
    DateTime CreatedOn,
    List<ListJobDto> Jobs
);

