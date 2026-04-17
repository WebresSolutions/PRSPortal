using Microsoft.AspNetCore.Mvc;
using Nextended.Core.Extensions;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.Quote;
using Portal.Shared.DTO.Quote.PartailQuote;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Endpoints;

public static class ClientEndpoints
{
    /// <summary>
    /// Add endpoints for client use. 
    /// These endpoint will be anonymous and the user will be required to supply a valid and in date token to access these endpoints.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication AddClientEndpoints(this WebApplication app)
    {
        app.MapGet("/api/client/partialquote",
            async (
                [FromServices] IQuoteService quoteService,
                [FromQuery] string token,
                HttpContext httpContext
            ) =>
            {
                if (token.IsNullOrEmpty())
                    return Results.BadRequest("Invalid Token Supplied");

                Result<QuotePartialDetailsDto> result = await quoteService.GetQuoteDetailsUnauthenticated(httpContext, token);
                return EndpointsHelper.ProcessResult(result, "An error occurred while getting quote details");
            })
            .WithSummary("Get quote details for client acceptance link")
            .WithDescription("Returns a reduced quote payload when a valid quote access token is supplied (no auth).")
            .Produces<QuotePartialDetailsDto>()
            .AllowAnonymous();

        app.MapPut("/api/client/submitquote",
            async (
                [FromServices] IQuoteService quoteService,
                [FromQuery] string token,
                [FromBody] ClientQuoteSubmissionDto data,
                HttpContext httpContext
            ) =>
            {
                if (token.IsNullOrEmpty())
                    return Results.BadRequest("Invalid Token Supplied");

                Result<QuotePartialDetailsDto> result = await quoteService.SubmitQuoteResponse(token, data, httpContext);
                return EndpointsHelper.ProcessResult(result, "An error occurred while submitting quote details.");
            })
            .WithSummary("Submits quote details.")
            .WithDescription("Returns partial quote details.")
            .Produces<QuotePartialDetailsDto>()
            .AllowAnonymous();

        return app;
    }
}
