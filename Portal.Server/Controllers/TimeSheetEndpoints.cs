using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Controllers;

/// <summary>
/// Static class containing schedule-related API endpoint definitions
/// Provides RESTful endpoints for schedule operations
/// </summary>
public static class TimeSheetEndpoints
{
    public static void AddTimeSheetendpoints(this WebApplication app, bool reqAuth = true)
    {
        RouteGroupBuilder appGroup = app.MapGroup("/api/timesheet");

        appGroup.MapGet("{userId}",
            async (
                [FromRoute] int userId,
                [FromQuery] DateTime start,
                [FromQuery] DateTime? end,
                [FromServices] ITimeSheetService timesheetService,
                HttpContext httpContext
                ) =>
            {
                Result<TimeSheetDto[]> result = await timesheetService.GetUserTimeSheets(httpContext, start, end, userId);
                return EndpointsHelper.ProcessResult(result, "An Error occured getting user timesheets");
            })
            .WithSummary("Get timesheet")
            .WithDescription("Returns timesheet data. If id is not provided, uses the current user's ID from the auth context.");

        if (reqAuth)
            appGroup.RequireAuthorization();
        else
            appGroup.AllowAnonymous();
    }
}
