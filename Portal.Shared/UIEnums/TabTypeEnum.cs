using System.ComponentModel.DataAnnotations;

namespace Portal.Shared;

public enum TabTypeEnum
{
    [Display(Name = "All")]
    All = 1,
    [Display(Name = "Action Required")]
    ActionRequired = 2,
    [Display(Name = "Deleted")]
    Deleted = 3,
    [Display(Name = "Unsent")]
    Unsent = 4,
    [Display(Name = "Overdue")]
    Overdue = 5,
    [Display(Name = "Unpaid")]
    Unpaid = 6,
    [Display(Name = "Completed")]
    Completed = 7,
    [Display(Name = "Active")]
    Active = 8,
    [Display(Name = "To Invoice")]
    ToInvoice = 9,
    [Display(Name = "Track")]
    ByTrack = 10,
    [Display(Name = "Time")]
    ByDate = 11,
    [Display(Name = "Company")]
    Company = 12,
    [Display(Name = "Individual")]
    Individual = 13,
    [Display(Name = "Survey")]
    Survey = 14,
    [Display(Name = "Construction")]
    Construction = 15
}
