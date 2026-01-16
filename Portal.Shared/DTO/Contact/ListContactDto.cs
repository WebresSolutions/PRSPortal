using Portal.Shared.DTO.Address;

namespace Portal.Shared.DTO.Contact;

/// <summary>
/// Data transfer object representing contact information for list views
/// Contains essential contact details for display in contact listing pages
/// </summary>
public class ListContactDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the contact
    /// </summary>
    public int ContactId { get; set; }

    /// <summary>
    /// Gets or sets the full name of the contact
    /// </summary>
    public string FullName { get; set; } = "";

    /// <summary>
    /// Gets or sets the email address of the contact
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>
    /// Gets or sets the phone number of the contact
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the address information for the contact
    /// </summary>
    public AddressDTO? Address { get; set; }

    /// <summary>
    /// Gets or sets the parent contact information if this is a child contact
    /// </summary>
    public ContactDto? ParentContact { get; set; }

    /// <summary>
    /// Initializes a new instance of the ListContactDto class
    /// </summary>
    /// <param name="contactId">The unique identifier for the contact</param>
    /// <param name="fullName">The full name of the contact</param>
    /// <param name="email">The email address of the contact</param>
    /// <param name="phone">The phone number of the contact</param>
    /// <param name="address">The address information</param>
    /// <param name="parentContact">The parent contact information if applicable</param>
    public ListContactDto(
        int contactId,
        string fullName,
        string email,
        string? phone,
        AddressDTO? address,
        ContactDto? parentContact)
    {
        ContactId = contactId;
        FullName = fullName;
        Email = email;
        Phone = phone;
        Address = address;
        ParentContact = parentContact;
    }

    /// <summary>
    /// Parameterless constructor for serialization
    /// </summary>
    public ListContactDto()
    {
    }
}

