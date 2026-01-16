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
    public static void AddContactEndpoints(this WebApplication app)
    {
        RouteGroupBuilder appGroup = app.MapGroup("/api/contacts");

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
        });

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
        });

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
        });
    }
}

