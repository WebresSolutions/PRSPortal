using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.DataEnums;

/// <summary>
/// Enumeration representing job type categories
/// </summary>
public enum JobTypeEnum
{
    /// <summary>
    /// Construction job type
    /// </summary>
    [Display(Name = "Construction")]
    Construction = 1,
    /// <summary>
    /// Surveying job type
    /// </summary>
    [Display(Name = "Surveying")]
    Surveying = 2,
}
