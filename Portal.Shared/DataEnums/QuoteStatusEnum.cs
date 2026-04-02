using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.DataEnums;

public enum QuoteStatusEnum
{
    /// <summary>
    /// Construction job type
    /// </summary>
    [Display(Name = "Draft")]
    Draft = 1,
    /// <summary>
    /// Construction job type
    /// </summary>
    [Display(Name = "New")]
    New = 2,
    /// <summary>
    /// Construction job type
    /// </summary>
    [Display(Name = "Sent")]
    Sent = 3,
    /// <summary>
    /// Construction job type
    /// </summary>
    [Display(Name = "Lost")]
    Lost = 4,
    /// <summary>
    /// Construction job type
    /// </summary>
    [Display(Name = "Rejected")]
    Rejected = 5,
    /// <summary>
    /// Construction job type
    /// </summary>
    [Display(Name = "Accepted")]
    Accepted = 6
}