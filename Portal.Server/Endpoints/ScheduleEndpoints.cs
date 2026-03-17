using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.ResponseModels;
using Quartz.Util;

namespace Portal.Server.Controllers;

/// <summary>
/// Static class containing schedule-related API endpoint definitions
/// Provides RESTful endpoints for schedule operations
/// </summary>
public static class ScheduleEndpoints
{
    /// <summary>
    /// Registers schedule-related API endpoints with the application
    /// </summary>
    /// <param name="app">The web application to register endpoints with</param>
    public static void AddScheduleEndpoints(this WebApplication app, bool reqAuth = true)
    {
        RouteGroupBuilder appGroup = app.MapGroup("/api/schedule").RequireAuthorization();

        // Gets all jobs with pagination and optional filtering/sorting
        appGroup.MapGet("slots", async (
            [FromServices] IScheduleService schService,
            [FromQuery] DateOnly date,
            [FromQuery] JobTypeEnum jobType,
            HttpContext httpContext
            ) =>
        {
            Result<List<ScheduleTrackDto>> result = await schService.GetScheduleSlotsForDate(httpContext, date, jobType);
            return EndpointsHelper.ProcessResult(result, "An Error occured while loading schedule slots");
        })
            .WithSummary("Get schedule slots for date")
            .WithDescription("Returns schedule slots for a given date and job type. Requires date and jobType query parameters.")
            .Produces<List<ScheduleTrackDto>>();

        // Gets schedule colours
        appGroup.MapGet("colours", async (
            [FromServices] IScheduleService schService,
            HttpContext httpContext
            ) =>
        {
            Result<List<ScheduleColourDto>> result = await schService.GetScheduleColours();
            return EndpointsHelper.ProcessResult(result, "An error occured while loading colours");
        })
            .WithSummary("Get schedule colours")
            .WithDescription("Returns the list of schedule colour definitions used for calendar/schedule display.")
            .Produces<List<ScheduleColourDto>>();

        // Update a schedule colour
        appGroup.MapPut("colours", async (
            [FromServices] IScheduleService schService,
            [FromBody] ScheduleColourDto colour,
            HttpContext httpContext
            ) =>
        {
            StringNormalizer.Normalize(colour);
            if (colour.ColourHex.IsNullOrWhiteSpace() || !colour.ColourHex.StartsWith('#'))
                return Results.BadRequest(new { Message = "Invalid ScheduleColour" });

            Result<ScheduleColourDto> result = await schService.UpdateScheduleColour(colour);
            return EndpointsHelper.ProcessResult(result, "An error occured while loading colours");
        })
            .WithSummary("Update schedule colour")
            .WithDescription("Updates a schedule colour. ColourHex must start with '#' and be non-empty. Returns 400 for invalid colour.")
            .Produces<ScheduleColourDto>();

        // Get a single schedule by id
        appGroup.MapGet("{id}", async (
            [FromServices] IScheduleService schService,
            [FromRoute] int id,
            HttpContext httpContext
            ) =>
        {
            if (id <= 0)
                return Results.BadRequest(new { Message = "Invalid schedule id." });
            Result<ScheduleDto> result = await schService.GetSchedule(id);
            return EndpointsHelper.ProcessResult(result, "An error occurred while loading the schedule");
        })
            .WithSummary("Get schedule by id")
            .WithDescription("Returns a single schedule by its id.")
            .Produces<ScheduleDto>();

        // Create or update a schedule (single calendar entry)
        appGroup.MapPut("", async (
            [FromServices] IScheduleService schService,
            [FromBody] UpdateScheduleDto data,
            HttpContext httpContext
            ) =>
        {
            StringNormalizer.Normalize(data);
            Result<int> result = await schService.UpdateSchedule(httpContext, data);
            return EndpointsHelper.ProcessResult(result, "An error occurred while saving the schedule");
        })
            .WithSummary("Update schedule")
            .WithDescription("Creates a new schedule when Id is 0, or updates/soft-deletes an existing schedule.")
            .Produces<int>();

        // Create or update a schedule track (day slot with assigned users)
        appGroup.MapPut("tracks", async (
            [FromServices] IScheduleService schService,
            [FromBody] UpdateScheduleTrackDto data,
            HttpContext httpContext
            ) =>
        {
            Result<ScheduleTrackDto> result = await schService.UpdateScheduleTrack(httpContext, data);
            return EndpointsHelper.ProcessResult(result, "An error occurred while saving the schedule track");
        })
            .WithSummary("Update schedule track")
            .WithDescription("Creates a new schedule track when ScheduleTrackId is 0, or updates assigned users and date for an existing track.")
            .Produces<ScheduleTrackDto>();

        appGroup.MapDelete("tracks/{id}", async (
            [FromServices] IScheduleService schService,
            [FromRoute] int id,
            HttpContext httpContext) =>
        {
            if (id <= 0)
                return Results.BadRequest($"Invalid Id: {id}");

            Result<int> result = await schService.DeleteTrack(httpContext, id);
            return EndpointsHelper.ProcessResult(result, "An error occurred while saving the schedule track");
        })
            .WithSummary("Soft delete a schedule track")
            .WithDescription("Sets the deleted at on the schedule track")
            .Produces<int>();

        // Get weekly schedule
        appGroup.MapGet("week", async (
            [FromServices] IScheduleService schService,
            [FromQuery] JobTypeEnum jobType,
            [FromQuery] DateOnly? weekDay
            ) =>
        {
            Result<WeeklyGroupedByScheduleDto[]> result = await schService.GetWeeklySchedule(jobType, weekDay);
            return EndpointsHelper.ProcessResult(result, "An error occurred while loading the weekly schedule");
        })
            .WithSummary("Get weekly schedule")
            .WithDescription("Returns all schedule entries for the week containing the given date (or current week if weekDay is omitted), filtered by job type.")
            .Produces<WeeklyGroupedByScheduleDto[]>();

        if (reqAuth)
            appGroup.RequireAuthorization();
        else
            appGroup.AllowAnonymous();
    }
}
