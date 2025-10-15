using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Portal.Server.Helpers;

public sealed class FormValueRequiredAttribute : ActionMethodSelectorAttribute
{
    private readonly string _name;

    public FormValueRequiredAttribute(string name)
    {
        _name = name;
    }

    public override bool IsValidForRequest(RouteContext context, ActionDescriptor action)
    {
        if (string.Equals(context.HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(context.HttpContext.Request.Method, "HEAD", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(context.HttpContext.Request.Method, "DELETE", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(context.HttpContext.Request.Method, "TRACE", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrEmpty(context.HttpContext.Request.ContentType))
        {
            return false;
        }

        return !context.HttpContext.Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)
            ? false
            : !string.IsNullOrEmpty(context.HttpContext.Request.Form[_name]);
    }
}
