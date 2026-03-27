using System.ComponentModel.DataAnnotations;

namespace Portal.Shared;

/// <summary>
/// Enumeration representing sort direction options
/// </summary>
public enum SortDirectionEnum
{
    /// <summary>
    /// Sort in ascending order
    /// </summary>
    [Display(Name = "asc")]
    Asc = 1,

    /// <summary>
    /// Sort in descending order
    /// </summary>
    [Display(Name = "desc")]
    Desc = 2,
}
