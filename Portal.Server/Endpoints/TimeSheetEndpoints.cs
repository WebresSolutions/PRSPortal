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
            .WithDescription("Returns timesheet data. If id is not provided, uses the current user's ID from the auth context.")
            .Produces<TimeSheetDto[]>();

        appGroup.MapPost("",
            async (
                [FromBody] TimeSheetDto entry,
                [FromServices] ITimeSheetService timesheetService,
                HttpContext httpContext
                ) =>
            {
                Result<TimeSheetDto> res = await timesheetService.AddTimeSheetEntry(httpContext, entry);
                return EndpointsHelper.ProcessResult(res, "An Error occured adding the timesheet entry");
            })
            .WithSummary("Add timesheet entry")
            .WithDescription("Adds a new timesheet entry for the current user.")
            .Produces<TimeSheetDto>();

        appGroup.MapPut("",
            async (
                [FromBody] TimeSheetDto entry,
                [FromServices] ITimeSheetService timesheetService,
                HttpContext httpContext
                ) =>
            {
                Result<TimeSheetDto> res = await timesheetService.UpdateTimeSheet(httpContext, entry);
                return EndpointsHelper.ProcessResult(res, "An Error occured updating the timesheet entry");
            })
            .WithSummary("Update timesheet entry")
            .WithDescription("Updates an existing timesheet entry by ID.")
            .Produces<TimeSheetDto>();

        appGroup.MapDelete("{id}",
            async (
                [FromRoute] int id,
                [FromServices] ITimeSheetService timesheetService,
                HttpContext httpContext
                ) =>
            {
                Result<bool> res = await timesheetService.RemoveTimeSheetEntry(httpContext, id);
                return EndpointsHelper.ProcessResult(res, "An Error occured removing the timesheet entry");
            })
            .WithSummary("Remove timesheet entry")
            .WithDescription("Removes a timesheet entry by ID.")
            .Produces<bool>();

        if (reqAuth)
            appGroup.RequireAuthorization();
        else
            appGroup.AllowAnonymous();
    }
}
