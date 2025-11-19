using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Controllers;

public static class JobEndpoints
{
    public static void AddJobEndpoints(this WebApplication app)
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
        }).AllowAnonymous();
    }
}
