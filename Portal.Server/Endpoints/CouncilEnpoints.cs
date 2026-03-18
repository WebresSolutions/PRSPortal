using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Councils;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Controllers;

public static class CouncilEnpoints
{
    /// <summary>
    /// Registers council-related API endpoints with the application.
    /// </summary>
    /// <param name="app">The web application to register endpoints with</param>
    public static void AddCouncilEndpoints(this WebApplication app, bool reqAuth = true)
    {
        RouteGroupBuilder appGroup = app.MapGroup("/api/councils").RequireAuthorization();

        appGroup.MapGet("", async (
            [FromServices] ICouncilService councilService
            ) =>
        {
            Result<CouncilPartialDto[]> result = await councilService.GetCouncils();
            return EndpointsHelper.ProcessResult(result, "An error occured while getting all councils");
        })
            .WithSummary("List councils")
            .WithDescription("Returns a list of all councils (partial DTOs).")
            .Produces<CouncilPartialDto[]>();

        appGroup.MapGet("{councilId}", async (
            [FromServices] ICouncilService councilService,
            [FromRoute] int councilId
            ) =>
        {
            if (councilId <= 0)
                return Results.BadRequest($"Invalid council Id {councilId}. Value must be greater than 0.");

            Result<CouncilDetailsDto> result = await councilService.GetCouncilDetails(councilId);
            return EndpointsHelper.ProcessResult(result, "An error occurred getting council details");
        })
            .WithSummary("Get council by ID")
            .WithDescription("Returns full details for a single council by council ID. Returns 400 if councilId is invalid.")
            .Produces<CouncilDetailsDto>();

        appGroup.MapPost("", async (
            [FromServices] ICouncilService councilService,
            [FromBody] CouncilCreationDto data,
            HttpContext httpContext
            ) =>
        {
            StringNormalizer.Normalize(data);
            Result<int> result = await councilService.CreateCouncil(httpContext, data);
            return EndpointsHelper.ProcessResult(result, "An error occurred while creating the council");
        })
            .WithSummary("Create council")
            .WithDescription("Creates a new council with the provided details.")
            .Produces<int>();

        appGroup.MapPut("", async (
            [FromServices] ICouncilService councilService,
            [FromBody] CouncilUpdateDto data,
            HttpContext httpContext
            ) =>
        {
            StringNormalizer.Normalize(data);
            if (data.CouncilId <= 0)
                return Results.BadRequest("Invalid council Id");

            Result<CouncilDetailsDto> result = await councilService.UpdateCouncil(httpContext, data);
            return EndpointsHelper.ProcessResult(result, "An error occurred while updating the council");
        })
            .WithSummary("Update council")
            .WithDescription("Updates an existing council. Returns 400 if councilId is invalid.")
            .Produces<CouncilDetailsDto>();

        if (reqAuth)
            appGroup.RequireAuthorization();
        else
            appGroup.AllowAnonymous();
    }
}
