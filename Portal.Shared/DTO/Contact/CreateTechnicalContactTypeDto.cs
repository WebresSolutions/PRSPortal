using System.ComponentModel.DataAnnotations;
using static Portal.Shared.ValidationAttributes;

namespace Portal.Shared.DTO.Contact;

public class SaveTechnicalContactTypeDto
{
    [Required]
    [NotZero]
    public int ContactId { get; set; }

    [Required]
    [NotZero]
    public int ContactTypeId { get; set; } = 1;

    [Required]
    [NotZero]
    public int JobId { get; set; }

    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    /// <summary>
    /// Initializes a new instance of the SaveTechnicalContactTypeDto class. 
    /// </summary>
    public SaveTechnicalContactTypeDto()
    {
    }

    /// <summary>
    /// For converting from a TechnicalContactDto to a SaveTechnicalContactTypeDto when editing an existing technical contact. This allows us to reuse the same dialog for both creating and editing technical contacts,
    /// as the SaveTechnicalContactTypeDto contains all the necessary fields for both operations.
    /// </summary>
    /// <param name="dto"></param>
    public SaveTechnicalContactTypeDto(TechnicalContactDto dto)
    {
        ContactId = dto.ContactId;
        ContactTypeId = dto.ContactTypeId;
        JobId = dto.JobId;
        ContactName = dto.ContactName;
        Email = dto.Email;
        Phone = dto.Phone;
    }
}
