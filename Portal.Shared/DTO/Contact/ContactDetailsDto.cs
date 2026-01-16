using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Job;

namespace Portal.Shared.DTO.Contact;

/// <summary>
/// Data transfer object representing detailed contact information
/// Contains complete contact details including address and related jobs
/// </summary>
public record ContactDetailsDto(
    int contactId,
    string fullName,
    string firstName,
    string lastName,
    string email,
    string? phone,
    string? fax,
    AddressDTO? address,
    ContactDto? parentContact,
    string createdBy,
    DateTime createdOn,
    List<ListJobDto> jobs
);

