using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Councils;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Controllers;

public static class CouncilEnpoints
{/// <summary>
 /// Registers job-related API endpoints with the application
 /// </summary>
 /// <param name="app">The web application to register endpoints with</param>
    public static void AddCouncilEndpoints(this WebApplication app, bool reqAuth = true)
    {
        RouteGroupBuilder appGroup = app.MapGroup("/api/councils").RequireAuthorization();

        // Gets all jobs with pagination and optional filtering/sorting
        appGroup.MapGet("", async (
            [FromServices] ICouncilService councilService
            ) =>
        {

            Result<CouncilPartialDto[]> result = await councilService.GetCouncils();
            return EndpointsHelper.ProcessResult(result, "An error occured while getting all councils");
        })
            .WithSummary("List councils")
            .WithDescription("Returns a list of all councils (partial DTOs).");

        // Gets council details without jobs
        appGroup.MapGet("{councilId}", async (
            [FromServices] ICouncilService councilService,
            [FromRoute] int councilId,
            HttpContext httpContext
            ) =>
        {
            if (councilId <= 0)
                return Results.BadRequest($"Invalid council Id{councilId}. Value must be greater than 0.");

            Result<CouncilDetailsDto> result = await councilService.GetCouncilDetails(councilId);
            return EndpointsHelper.ProcessResult(result, "An Error occurred getting council details");
        })
            .WithSummary("Get council by ID")
            .WithDescription("Returns full details for a single council by council ID. Returns 400 if councilId is invalid.");

        // Gets jobs for a specific council with pagination
        appGroup.MapGet("{councilId}/jobs", async (
            [FromServices] ICouncilService councilService,
            [FromRoute] int councilId,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? orderby,
            [FromQuery] SortDirectionEnum? order,
            HttpContext httpContext
            ) =>
        {
            if (councilId <= 0)
                return Results.BadRequest($"Invalid council Id{councilId}. Value must be greater than 0.");

            if (page <= 0)
                page = 1;

            order ??= SortDirectionEnum.Desc;

            Result<PagedResponse<ListJobDto>> result = await councilService.GetCouncilJobs(councilId, page, pageSize, order, orderby);
            return EndpointsHelper.ProcessResult(result, "An Error occurred getting council jobs");
        })
            .WithSummary("Get jobs for a council")
            .WithDescription("Returns a paginated list of jobs associated with the specified council. Supports ordering via orderby and order query parameters.");

        if (reqAuth)
            appGroup.RequireAuthorization();
        else
            appGroup.AllowAnonymous();
    }
}
