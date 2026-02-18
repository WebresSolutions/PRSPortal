using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Controllers;

/// <summary>
/// Static class containing contact-related API endpoint definitions
/// Provides RESTful endpoints for contact operations
/// </summary>
public static class ContactEndpoints
{
    /// <summary>
    /// Registers contact-related API endpoints with the application
    /// </summary>
    /// <param name="app">The web application to register endpoints with</param>
    public static void AddContactEndpoints(this WebApplication app, bool reqAuth = true)
    {
        RouteGroupBuilder appGroup = app.MapGroup("/api/contacts").RequireAuthorization();

        // Gets all contacts with pagination and optional filtering/sorting
        appGroup.MapGet("", async (
            [FromServices] IContactService contactService,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? searchFilter,
            [FromQuery] string? orderby,
            [FromQuery] SortDirectionEnum? order,
            HttpContext httpContext
            ) =>
        {
            if (page <= 0)
                page = 1;

            order ??= SortDirectionEnum.Asc;

            Result<PagedResponse<ListContactDto>> result = await contactService.GetAllContacts(page, pageSize, order, searchFilter, orderby);
            return EndpointsHelper.ProcessResult(result, "An error occurred while loading contacts");
        })
            .WithSummary("List contacts")
            .WithDescription("Returns a paginated list of contacts with optional search filter and sorting by page, pageSize, searchFilter, orderby, and order.")
            .Produces<PagedResponse<ListContactDto>>();

        // Gets contact details without jobs
        appGroup.MapGet("{contactId}", async (
            [FromServices] IContactService contactService,
            [FromRoute] int contactId,
            HttpContext httpContext
            ) =>
        {
            if (contactId <= 0)
                return Results.BadRequest($"Invalid contact Id {contactId}. Value must be greater than 0.");

            Result<ContactDetailsDto> result = await contactService.GetContactDetails(contactId);
            return EndpointsHelper.ProcessResult(result, "An error occurred getting contact details");
        })
            .WithSummary("Get contact by ID")
            .WithDescription("Returns full details for a single contact by contact ID. Returns 400 if contactId is invalid.")
            .Produces<ContactDetailsDto>();

        // Gets jobs for a specific contact with pagination
        appGroup.MapGet("{contactId}/jobs", async (
            [FromServices] IContactService contactService,
            [FromRoute] int contactId,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? orderby,
            [FromQuery] SortDirectionEnum? order,
            HttpContext httpContext
            ) =>
        {
            if (contactId <= 0)
                return Results.BadRequest($"Invalid contact Id {contactId}. Value must be greater than 0.");

            if (page <= 0)
                page = 1;

            order ??= SortDirectionEnum.Desc;

            Result<PagedResponse<ListJobDto>> result = await contactService.GetContactJobs(contactId, page, pageSize, order, orderby);
            return EndpointsHelper.ProcessResult(result, "An error occurred while getting contact jobs");
        })
            .WithSummary("Get jobs for a contact")
            .WithDescription("Returns a paginated list of jobs associated with the specified contact. Supports ordering via orderby and order query parameters.")
            .Produces<PagedResponse<ListJobDto>>();

        if (reqAuth)
            appGroup.RequireAuthorization();
        else
            appGroup.AllowAnonymous();
    }
}

