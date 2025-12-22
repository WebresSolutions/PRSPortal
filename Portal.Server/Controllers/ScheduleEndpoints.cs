using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.ResponseModels;
using Quartz.Util;

namespace Portal.Server.Controllers;

public static class ScheduleEndpoints
{
    public static void AddScheduleEndpoints(this WebApplication app)
    {
        RouteGroupBuilder appGroup = app.MapGroup("/api/schedule");

        // Gets all jobs with pagination and optional filtering/sorting
        appGroup.MapGet("slots", async (
            [FromServices] IScheduleService schService,
            [FromQuery] DateOnly date,
            [FromQuery] JobTypeEnum jobType,
            HttpContext httpContext
            ) =>
        {
            Result<List<ScheduleSlotDTO>> result = await schService.GetScheduleSlotsForDate(date, jobType);
            return EndpointsHelper.ProcessResult(result, "An Error occured while loading schedule slots");
        });

        // Gets all jobs with pagination and optional filtering/sorting
        appGroup.MapGet("colours", async (
            [FromServices] IScheduleService schService,
            HttpContext httpContext
            ) =>
        {
            Result<List<ScheduleColourDto>> result = await schService.GetScheduleColours();
            return EndpointsHelper.ProcessResult(result, "An error occured while loading colours");
        });

        // Gets all jobs with pagination and optional filtering/sorting
        appGroup.MapPut("colours", async (
            [FromServices] IScheduleService schService,
            [FromBody] ScheduleColourDto colour,
            HttpContext httpContext
            ) =>
        {
            if (colour.ColourHex.IsNullOrWhiteSpace() || !colour.ColourHex.StartsWith('#'))
                return Results.BadRequest(new { Message = "Invalid ScheduleColour" });

            Result<ScheduleColourDto> result = await schService.UpdateScheduleColour(colour);
            return EndpointsHelper.ProcessResult(result, "An error occured while loading colours");
        });
    }
}
