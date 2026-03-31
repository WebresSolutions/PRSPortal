using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.DTO;

/// <summary>
/// Enumeration representing job type categories
/// </summary>
public enum ContactTypeEnum
{
    /// <summary>
    /// Construction job type
    /// </summary>
    [Display(Name = "Company")]
    Company = 1,
    /// <summary>
    /// Surveying job type
    /// </summary>
    [Display(Name = "Individual")]
    Individual = 2,
}
