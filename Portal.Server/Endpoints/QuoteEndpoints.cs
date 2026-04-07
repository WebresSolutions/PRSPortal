using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.Quote;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Endpoints;

public static class QuoteEndpoints
{
    public static void AddQuoteEndpoints(this WebApplication app, string tags, bool reqAuth = true)
    {
        RouteGroupBuilder quoteEndpointGroup = app.MapGroup("/api/quotes").WithTags(tags);

        quoteEndpointGroup.MapGet(
            "", async (
                [FromServices] IQuoteService quoteService,
                [AsParameters] QuoteFilterDto filter) =>
            {
                Result<PagedResponse<QuoteListDto>> result = await quoteService.GetAllQuotes(filter);
                return EndpointsHelper.ProcessResult(result, "An Error occured while loading schedule slots");
            })
            .WithSummary("Get all quotes")
            .WithDescription("Returns a collection of Quotes. Optional flag for active quotes only")
            .Produces<PagedResponse<QuoteListDto>>();

        quoteEndpointGroup.MapGet("{quoteId}",
            async (
                [FromServices] IQuoteService quoteService,
                [FromRoute] int quoteId
            ) =>
            {
                if (quoteId <= 0)
                    return Results.BadRequest("Invalid quote ID");

                Result<QuoteDetailsDto> result = await quoteService.GetQuoteDetails(quoteId);
                return EndpointsHelper.ProcessResult(result, "An error occurred while getting the quote details");
            }).WithSummary("Get a quote by ID")
            .WithDescription("Returns the details of a quote by its ID")
            .Produces<QuoteDetailsDto>();

        quoteEndpointGroup.MapPost("",
            async (
                [FromServices] IQuoteService quoteService,
                [FromBody] QuoteCreationDto quoteCreateDto,
                HttpContext httpContext
            ) =>
            {
                Result<int> result = await quoteService.CreateNewQuote(quoteCreateDto, httpContext);
                return EndpointsHelper.ProcessResult(result, "An error occurred while creating the quote");
            }
        )
        .WithSummary("Create a new quote")
        .WithDescription("Creates a new quote with the provided details")
        .Produces<int>();

        quoteEndpointGroup.MapPut("",
            async (
                [FromServices] IQuoteService quoteService,
                [FromBody] QuoteUpdateDto quoteUpdateDto,
                HttpContext httpContext
            ) =>
            {
                Result<int> result = await quoteService.UpdateQuote(quoteUpdateDto, httpContext);
                return EndpointsHelper.ProcessResult(result, "An error occurred while updating the quote");
            })
            .WithSummary("Update a quote")
            .WithDescription("Updates an existing quote with the provided details")
            .Produces<int>();

        quoteEndpointGroup.MapDelete("{quoteId}",
            async (
                [FromServices] IQuoteService quoteService,
                [FromRoute] int quoteId,
                HttpContext httpContext
            ) =>
            {
                if (quoteId <= 0)
                    return Results.BadRequest("Invalid quote ID");

                Result<bool> result = await quoteService.DeleteQuote(quoteId, httpContext);
                return EndpointsHelper.ProcessResult(result, "An error occurred while deleting the quote");
            })
            .WithSummary("Delete a quote")
            .WithDescription("Deletes an existing quote by its ID")
            .Produces<int>();

        // Get the quoting templates 
        quoteEndpointGroup.MapGet("templates",
            async (
                [FromServices] IQuoteService quoteService
            ) =>
            {
                Result<QuoteTemplateDto[]> result = await quoteService.GetQuotingTemplates();
                return EndpointsHelper.ProcessResult(result, "An error occurred while getting the quoting templates");
            })
            .WithSummary("Get the quoting templates")
            .WithDescription("Returns the quoting templates for a given quote ID")
            .Produces<List<QuoteTemplateDto>>();

        quoteEndpointGroup.MapPut("templates",
            async (
                [FromServices] IQuoteService quoteService,
                [FromBody] QuoteTemplateDto quoteTemplateDto,
                HttpContext httpContext
            ) =>
            {
                Result<QuoteTemplateDto> result = quoteTemplateDto.Id is 0
                    ? await quoteService.CreateQuotingTemplate(quoteTemplateDto, httpContext)
                    : await quoteService.UpdateQuotingTemplate(quoteTemplateDto, httpContext);

                return EndpointsHelper.ProcessResult(result, "An error occurred while creating the quoting template");
            })
            .WithSummary("Create or update a quoting template")
            .WithDescription("Creates a new quoting template with the provided details. Updates it if the id is not 0")
            .Produces<QuoteTemplateDto>();

        quoteEndpointGroup.MapDelete("templates/{quoteId}",
            async (
                [FromServices] IQuoteService quoteService,
                [FromRoute] int quoteId
            ) =>
            {
                Result<bool> result = await quoteService.DeleteQuotingTemplate(quoteId);
                return EndpointsHelper.ProcessResult(result, "An error occurred while deleting the quoting template");
            })
            .WithSummary("Delete a quoting template")
            .WithDescription("Deletes an existing quoting template by its ID")
            .Produces<int>();

        if (reqAuth)
            quoteEndpointGroup.RequireAuthorization();
    }
}
