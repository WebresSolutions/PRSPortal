using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Portal.Server.Helpers;

/// <summary>
/// Action method selector attribute that requires a specific form value to be present
/// Used to differentiate between action methods based on form data
/// </summary>
public sealed class FormValueRequiredAttribute : ActionMethodSelectorAttribute
{
    /// <summary>
    /// The name of the required form value
    /// </summary>
    private readonly string _name;

    /// <summary>
    /// Initializes a new instance of the FormValueRequiredAttribute class
    /// </summary>
    /// <param name="name">The name of the form value that must be present</param>
    public FormValueRequiredAttribute(string name)
    {
        _name = name;
    }

    /// <summary>
    /// Determines whether the action method is valid for the current request
    /// Checks if the required form value is present in the request
    /// </summary>
    /// <param name="context">The route context</param>
    /// <param name="action">The action descriptor</param>
    /// <returns>True if the form value is present and the request method supports form data, otherwise false</returns>
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
