namespace Portal.Shared.DTO.Job;

/// <summary>
/// Data transfer object representing contact information for a job
/// Contains contact details including name, email, and phone
/// </summary>
public class JobContactDto
{
    public JobContactDto() { }

    public JobContactDto(int contactId, string contactName, string email, string phone)
    {
        ContactId = contactId;
        ContactName = contactName;
        Email = email;
        Phone = phone;
    }
    public int ContactId { get; set; }
    /// <summary>
    /// Gets or sets the name of the contact
    /// </summary>
    public string ContactName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the email address of the contact
    /// </summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the phone number of the contact
    /// </summary>
    public string Phone { get; set; } = string.Empty;

}
