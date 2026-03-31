using Portal.Shared.DTO.Address;

namespace Portal.Shared.DTO.Contact;

/// <summary>
/// Data transfer object representing detailed contact information
/// Contains complete contact details including address and related jobs
/// </summary>
public record ContactDetailsDto(
    int ContactId,
    int ContactType,
    string ContactTypeName,
    string FullName,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Fax,
    AddressDto? Address,
    ContactDto? ParentContact,
    string CreatedBy,
    DateTime CreatedOn,
    int JobCount,
    int TechContactCount,
    int InvoicesCount,
    int SubContactsCount,
    SubContactDto[] SubContacts
);

