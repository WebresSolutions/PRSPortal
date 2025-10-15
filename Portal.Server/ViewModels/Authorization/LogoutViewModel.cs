using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Portal.Server.ViewModels.Authorization;

public class LogoutViewModel
{
    [BindNever]
    public string RequestId { get; set; }
}
