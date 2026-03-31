using System.ComponentModel.DataAnnotations;
using Portal.Shared.DTO.Address;

namespace Portal.Shared.DTO.Councils;

/// <summary>
/// Data transfer object for creating a new council.
/// </summary>
public class CouncilCreationDto
{
    [Required(ErrorMessage = "Council name is required")]
    [MaxLength(255, ErrorMessage = "Council name cannot exceed 255 characters")]
    public string CouncilName { get; set; } = "";

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(50)]
    public string? Fax { get; set; }

    [MaxLength(255)]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    public AddressDto? Address { get; set; }
}
