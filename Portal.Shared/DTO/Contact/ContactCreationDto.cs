using System.ComponentModel.DataAnnotations;
using Portal.Shared.DTO.Address;

namespace Portal.Shared.DTO.Contact;

/// <summary>
/// Data transfer object for creating a new contact.
/// </summary>
public class ContactCreationDto
{
    /// <summary>
    /// The contact type identifier.
    /// </summary>
    [Required(ErrorMessage = "Contact type is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Contact type is required")]
    public int TypeId { get; set; }

    /// <summary>
    /// Optional parent contact identifier for sub-contacts.
    /// </summary>
    public int? ParentContactId { get; set; }

    /// <summary>
    /// First name of the contact.
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string FirstName { get; set; } = "";

    /// <summary>
    /// Last name of the contact.
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string LastName { get; set; } = "";

    /// <summary>
    /// Email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [MaxLength(255)]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = "";

    /// <summary>
    /// Phone number.
    /// </summary>
    [MaxLength(50)]
    public string? Phone { get; set; }

    /// <summary>
    /// Fax number.
    /// </summary>
    [MaxLength(50)]
    public string? Fax { get; set; }

    /// <summary>
    /// Optional address for the contact.
    /// </summary>
    public AddressDTO? Address { get; set; }
}
