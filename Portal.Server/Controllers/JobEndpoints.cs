using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Controllers;

/// <summary>
/// Static class containing job-related API endpoint definitions
/// Provides RESTful endpoints for job operations
/// </summary>
public static class JobEndpoints
{
    /// <summary>
    /// Registers job-related API endpoints with the application
    /// </summary>
    /// <param name="app">The web application to register endpoints with</param>
    public static void AddJobEndpoints(this WebApplication app, bool reqAuth = true)
    {
        RouteGroupBuilder appGroup = app.MapGroup("/api/jobs");

        // Gets all jobs with pagination and optional filtering/sorting
        appGroup.MapGet("", async (
            [FromServices] IJobService jobService,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? nameFilter,
            [FromQuery] string? orderby,
            [FromQuery] SortDirectionEnum? order,
            HttpContext httpContext
            ) =>
        {

            if (page <= 0)
                page = 1;

            order ??= SortDirectionEnum.Asc;

            Result<PagedResponse<ListJobDto>> result = await jobService.GetAllJobs(page, pageSize, order, nameFilter, orderby);
            return EndpointsHelper.ProcessResult(result, "An Error occured while loading facilities");
        });

        // Gets all jobs with pagination and optional filtering/sorting
        appGroup.MapGet("{jobId}", async (
            [FromServices] IJobService jobService,
            [FromRoute] int jobId,
            HttpContext httpContext
            ) =>
        {
            if (jobId <= 0)
                return Results.BadRequest("Bad Request");

            Result<JobDetailsDto> result = await jobService.GetJob(jobId);
            return EndpointsHelper.ProcessResult(result, "An Error occured while loading facilities");
        });

        appGroup.MapGet("notes/{userId}", async (
            [FromServices] IJobService jobService,
            [FromRoute] int userId,
            [FromQuery] bool includeDeleted,
            HttpContext httpContext
            ) =>
        {
            if (userId < 0)
                return Results.BadRequest("Bad Request");

            Result<List<JobNoteDto>> result = await jobService.GetUserAssignedJobsNotes(httpContext, userId, includeDeleted);
            return EndpointsHelper.ProcessResult(result, "An Error occured while loading facilities");
        });

        if (reqAuth)
            appGroup.RequireAuthorization();
        else
            appGroup.AllowAnonymous();
    }

}
