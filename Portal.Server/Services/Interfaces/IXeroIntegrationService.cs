using Microsoft.AspNetCore.Http;
using Xero.NetStandard.OAuth2.Model.PayrollAu;

namespace Portal.Server.Services.Interfaces;

public interface IXeroIntegrationService
{
    /// <summary>Builds the Xero OAuth authorization URL. Frontend should redirect the user here.</summary>
    /// <param name="state">Optional state for CSRF; if null, a random state is generated and stored.</param>
    string GetAuthorizationUrl(string? state = null);

    /// <summary>Handles the OAuth callback: exchanges code for tokens, stores them, then redirects to frontend.</summary>
    /// <returns>True if callback was handled successfully.</returns>
    Task<bool> HandleCallbackAsync(HttpContext context, CancellationToken cancellationToken = default);

    /// <summary>Returns a valid access token and tenant id, refreshing the token if expired.</summary>
    Task<(string? AccessToken, string? TenantId)> GetValidAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>Whether a Xero connection (refresh token) is stored.</summary>
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default);

    /// <summary>Disconnects Xero by removing stored tokens.</summary>
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    Task<LeaveApplications> GetLeaveApplications();
}