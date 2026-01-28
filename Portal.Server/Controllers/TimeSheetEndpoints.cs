using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Portal.Server.Controllers;

/// <summary>
/// Static class containing schedule-related API endpoint definitions
/// Provides RESTful endpoints for schedule operations
/// </summary>
public static class TimeSheetEndpoints
{
    public static void TimeSheetendpoints(this WebApplication app, bool reqAuth = true)
    {
        RouteGroupBuilder appGroup = app.MapGroup("/api/timesheet");

        appGroup.MapGet("",
            async (
                [FromQuery] string? id,
                HttpContext httpContext
                ) =>
        {
            if (id is null)
                id = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ;
        });

        if (reqAuth)
            appGroup.RequireAuthorization();
        else
            appGroup.AllowAnonymous();
    }
}
