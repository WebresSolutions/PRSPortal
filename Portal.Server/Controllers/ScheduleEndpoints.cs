using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.ResponseModels;

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
            return EndpointsHelper.ProcessResult(result, "An Error occured while loading facilities");
        }).AllowAnonymous();
    }
}
