namespace Portal.Shared.DTO.Contact;

/// <summary>
/// Data transfer object representing contact information
/// Contains basic contact identification and name
/// </summary>
/// <param name="contactId">The unique identifier for the contact</param>
/// <param name="fullName">The full name of the contact</param>
public record ContactDto(int contactId, string fullName);
