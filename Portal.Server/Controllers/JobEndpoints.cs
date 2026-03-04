using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.Contact;
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
           [AsParameters] JobFilterDto filter
       ) =>
        {
            // Simple validation to ensure page is always at least 1
            int validatedPage = filter.Page <= 0 ? 1 : filter.Page;
            filter = filter with { Page = validatedPage };

            Result<PagedResponse<ListJobDto>> result = await jobService.GetAllJobs(filter);

            return EndpointsHelper.ProcessResult(result, "An error occurred while loading jobs");
        })
       .WithSummary("List jobs")
       .WithDescription("Returns a paginated list of jobs filtered by address, contact, or job number.")
       .Produces<PagedResponse<ListJobDto>>();

        // Create a new job with the provided details
        appGroup.MapPost("", async (
            [FromServices] IJobService jobService,
            [FromBody] JobCreationDto data,
            HttpContext httpContext
            ) =>
        {
            Result<int> result = await jobService.CreateJob(httpContext, data);
            return EndpointsHelper.ProcessResult(result, "An Error occured while loading facilities");
        })
        .WithSummary("Create job")
        .WithDescription("Creates a new job with the provided details. Request body and implementation may be extended.")
        .Produces<int>();

        // Update a job with the provided details
        appGroup.MapPut("", async (
            [FromServices] IJobService jobService,
            [FromBody] JobDetailsDto data,
            HttpContext httpContext
            ) =>
        {
            if (data.JobId <= 0)
                return Results.BadRequest("Invalid Job Id");

            Result<JobDetailsDto> result = await jobService.UpdateJob(httpContext, data);
            return EndpointsHelper.ProcessResult(result, "An Error occured while loading facilities");
        })
        .WithSummary("Update job")
        .WithDescription("Updates an existing job by jobId. Returns 400 if jobId is invalid.")
        .Produces<JobDetailsDto>();

        // Update a job with the provided details
        appGroup.MapDelete("{id}", async (
            [FromServices] IJobService jobService,
            [FromRoute] int id,
            HttpContext httpContext
            ) =>
        {
            if (id <= 0)
                return Results.BadRequest("Invalid Job Id");

            Result<bool> result = await jobService.DeleteJob(httpContext, id);
            return EndpointsHelper.ProcessResult(result, "An Error occured while deleting Jobs.");
        })
        .WithSummary("Soft Delete job")
        .WithDescription("Deletes a Job and sets the deleted at date.")
        .Produces<JobDetailsDto>();

        // Gets notes for a job by job ID
        appGroup.MapGet("{jobId}/notes", async (
            [FromServices] IJobService jobService,
            [FromRoute] int jobId,
            [FromQuery] bool deleted = false,
            [FromQuery] bool? actionRequired = null
            ) =>
        {
            if (jobId <= 0)
                return Results.BadRequest("Invalid job Id");

            Result<List<JobNoteDto>> result = await jobService.GetJobNotes(jobId, deleted, actionRequired);
            return EndpointsHelper.ProcessResult(result, "An error occurred while loading job notes");
        })
        .WithSummary("Get job notes")
        .WithDescription("Returns notes for the specified job. Use includeDeleted query parameter to include soft-deleted notes.")
        .Produces<List<JobNoteDto>>();

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
            [FromQuery] bool? deleted,
            HttpContext httpContext
            ) =>
        {
            if (userId < 0)
                return Results.BadRequest("Bad Request");

            deleted ??= false;

            Result<List<JobNoteDto>> result = await jobService.GetUserAssignedJobsNotes(httpContext, userId, deleted.Value);
            return EndpointsHelper.ProcessResult(result, "An Error occured while loading facilities");
        })
        .WithSummary("Get notes for user's assigned jobs")
        .WithDescription("Returns notes for all jobs assigned to the specified user. Use includeDeleted query parameter to include soft-deleted notes.")
        .Produces<List<JobNoteDto>>();

        appGroup.MapPost("notes", async (
            [FromServices] IJobService jobService,
            [FromBody] JobNoteDto note,
            HttpContext httpContext
            ) =>
        {
            Result<List<JobNoteDto>> result;

            if (note.NoteId is 0)
                result = await jobService.CreateNote(httpContext, note);
            else
                result = await jobService.UpdateNote(httpContext, note);

            return EndpointsHelper.ProcessResult(result, "An Error occured while loading facilities");
        })
        .WithSummary("Create or update job note")
        .WithDescription("Creates a new note when NoteId is 0, otherwise updates the existing note.")
        .Produces<List<JobNoteDto>>();

        // Get technical contacts for a job and/or contact
        appGroup.MapGet("technical-contacts", async (
            [FromServices] IJobService jobService,
            [FromQuery] int? jobId,
            [FromQuery] int? contactId,
            [FromQuery] bool? deleted
            ) =>
        {
            if (jobId is null && contactId is null)
                return Results.BadRequest("Either jobId or contactId must be provided.");

            deleted ??= false;

            Result<TechnicalContactDto[]> result = await jobService.GetTechnicalContacts(jobId, contactId, deleted.Value);
            return EndpointsHelper.ProcessResult(result, "An error occurred while loading technical contacts");
        })
        .WithSummary("Get technical contacts")
        .WithDescription("Returns technical contacts filtered by jobId and/or contactId. At least one of jobId or contactId must be provided.")
        .Produces<TechnicalContactDto[]>();

        // Create a new technical contact (link a contact to a job with a role)
        appGroup.MapPut("technical-contacts", async (
            [FromServices] IJobService jobService,
            [FromBody] SaveTechnicalContactTypeDto dto,
            HttpContext httpContext
            ) =>
        {
            Result<TechnicalContactDto[]> result;
            if (dto.Id is 0)
                result = await jobService.NewTechnicalContact(httpContext, dto);
            else
                result = await jobService.UpdateTechnicalContact(httpContext, dto);

            return EndpointsHelper.ProcessResult(result, "An error occurred while creating the technical contact");
        })
        .WithSummary("Create or update technical contact")
        .WithDescription("Links a contact to a job with a specific role (technical contact type). Returns the updated list of technical contacts for the job.")
        .Produces<TechnicalContactDto[]>();

        if (reqAuth)
            appGroup.RequireAuthorization();
        else
            appGroup.AllowAnonymous();
    }

}
