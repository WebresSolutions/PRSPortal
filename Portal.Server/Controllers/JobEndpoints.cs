using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Job;
using System.Collections.Generic;
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
        })
            .WithSummary("List jobs")
            .WithDescription("Returns a paginated list of jobs with optional name filter and sorting by page, pageSize, nameFilter, orderby, and order.")
            .Produces<PagedResponse<ListJobDto>>();

        // Create a new job with the provided details
        appGroup.MapPost("", async (
            [FromServices] IJobService jobService,
            HttpContext httpContext
            ) =>
        {
            Result<PagedResponse<ListJobDto>> result = new();
            return EndpointsHelper.ProcessResult(result, "An Error occured while loading facilities");
        })
            .WithSummary("Create job")
            .WithDescription("Creates a new job with the provided details. Request body and implementation may be extended.")
            .Produces<PagedResponse<ListJobDto>>();

        // Update a job with the provided details
        appGroup.MapPut("{jobId}", async (
            [FromServices] IJobService jobService,
            [FromRoute] int jobId,
            HttpContext httpContext
            ) =>
        {
            if (jobId <= 0)
                jobId = 1;

            Result<PagedResponse<ListJobDto>> result = new();
            return EndpointsHelper.ProcessResult(result, "An Error occured while loading facilities");
        })
            .WithSummary("Update job")
            .WithDescription("Updates an existing job by jobId. Returns 400 if jobId is invalid.")
            .Produces<PagedResponse<ListJobDto>>();

        // Gets a single job by the ID.
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
        })
            .WithSummary("Get job by ID")
            .WithDescription("Returns full details for a single job by job ID. Returns 400 if jobId is invalid.")
            .Produces<JobDetailsDto>();

        // Gets notes for jobs assigned to a specific user, with optional inclusion of deleted notes
        appGroup.MapGet("assignedUserNotes/{userId}", async (
            [FromServices] IJobService jobService,
            [FromRoute] int userId,
            [FromQuery] bool? includeDeleted,
            HttpContext httpContext
            ) =>
        {
            if (userId < 0)
                return Results.BadRequest("Bad Request");

            includeDeleted ??= false;

            Result<List<JobNoteDto>> result = await jobService.GetUserAssignedJobsNotes(httpContext, userId, includeDeleted.Value);
            return EndpointsHelper.ProcessResult(result, "An Error occured while loading facilities");
        })
            .WithSummary("Get notes for user's assigned jobs")
            .WithDescription("Returns notes for all jobs assigned to the specified user. Use includeDeleted query parameter to include soft-deleted notes.")
            .Produces<List<JobNoteDto>>();

        if (reqAuth)
            appGroup.RequireAuthorization();
        else
            appGroup.AllowAnonymous();
    }

}
