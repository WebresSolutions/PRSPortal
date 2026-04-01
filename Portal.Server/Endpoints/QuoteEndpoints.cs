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

        if (reqAuth)
            quoteEndpointGroup.RequireAuthorization();
    }
}