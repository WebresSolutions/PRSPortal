using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.Contact;
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
    public static WebApplication AddContactEndpoints(this WebApplication app, string tags, bool reqAuth = true)
    {
        RouteGroupBuilder appGroup = app.MapGroup("/api/contacts").WithTags(tags);

        // Gets all contacts with pagination and optional filtering/sorting
        appGroup.MapGet("", async (
            [FromServices] IContactService contactService,
            [AsParameters] ContactFilterDto filter
            ) =>
        {
            int validatedPage = filter.Page <= 0 ? 1 : filter.Page;
            filter = filter with { Page = validatedPage };

            Result<PagedResponse<ListContactDto>> result = await contactService.GetAllContacts(filter);
            return EndpointsHelper.ProcessResult(result, "An error occurred while loading contacts");
        })
            .WithSummary("List contacts")
            .WithDescription("Returns a paginated list of contacts with optional search filters (name, email, phone, address, contactId) or searchFilter for type-ahead, and sorting.")
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

        // Create a new contact
        appGroup.MapPost("", async (
            [FromServices] IContactService contactService,
            [FromBody] ContactCreationDto data,
            HttpContext httpContext
            ) =>
        {
            StringNormalizer.Normalize(data);
            Result<int> result = await contactService.CreateContact(httpContext, data);
            return EndpointsHelper.ProcessResult(result, "An error occurred while creating the contact");
        })
            .WithSummary("Create contact")
            .WithDescription("Creates a new contact with the provided details.")
            .Produces<int>();

        // Update an existing contact
        appGroup.MapPut("", async (
            [FromServices] IContactService contactService,
            [FromBody] ContactUpdateDto data,
            HttpContext httpContext
            ) =>
        {
            StringNormalizer.Normalize(data);
            if (data.ContactId <= 0)
                return Results.BadRequest("Invalid contact Id");

            Result<ContactDetailsDto> result = await contactService.UpdateContact(httpContext, data);
            return EndpointsHelper.ProcessResult(result, "An error occurred while updating the contact");
        })
            .WithSummary("Update contact")
            .WithDescription("Updates an existing contact. Returns 400 if contactId is invalid.")
            .Produces<ContactDetailsDto>();

        if (reqAuth)
            appGroup.RequireAuthorization();
        else
            appGroup.AllowAnonymous();

        return app;
    }
}

