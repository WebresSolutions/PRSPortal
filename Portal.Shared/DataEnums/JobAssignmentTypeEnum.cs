using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.DataEnums;

public enum JobAssignementTypeEnum
{
    [Display(Name = "Current Owner")]
    CurrentOwner = 1,
    [Display(Name = "Responsible Member")]
    Responsible = 2
}