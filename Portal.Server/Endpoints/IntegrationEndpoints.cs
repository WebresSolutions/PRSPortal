using Microsoft.AspNetCore.Mvc;
using Portal.Server.Services.Interfaces;

namespace Portal.Server.Endpoints;

public static class IntegrationEndpoints
{
    public static WebApplication AddIntegrationEndpoints(this WebApplication app, string tags)
    {
        RouteGroupBuilder appGroup = app.MapGroup("/api/xero").WithTags(tags);
        // Xero OAuth callback - no auth; Xero redirects the browser here with ?code=...&state=...
        appGroup.MapGet("callback", async (
            [FromServices] IXeroIntegrationService xeroService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await xeroService.HandleCallbackAsync(httpContext, cancellationToken);
        }).AllowAnonymous();

        // Returns the Xero authorization URL; frontend redirects the user to this URL to start OAuth.
        appGroup.MapGet("authorize", (
            [FromServices] IXeroIntegrationService xeroService) =>
        {
            string url = xeroService.GetAuthorizationUrl();
            return Results.Ok(new { url });
        }).RequireAuthorization();

        // Whether Xero is connected (has stored refresh token).
        appGroup.MapGet("status", async (
            [FromServices] IXeroIntegrationService xeroService,
            CancellationToken cancellationToken) =>
        {
            bool connected = await xeroService.IsConnectedAsync(cancellationToken);
            return Results.Ok(new { connected });
        }).RequireAuthorization();

        // Disconnect Xero (remove stored tokens).
        appGroup.MapPost("disconnect", async (
            [FromServices] IXeroIntegrationService xeroService,
            CancellationToken cancellationToken) =>
        {
            await xeroService.DisconnectAsync(cancellationToken);
            return Results.Ok();
        }).RequireAuthorization();

        return app;
    }
}
