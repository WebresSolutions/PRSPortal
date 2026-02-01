using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Security.Claims;
using System.Text.Json;

namespace Portal.Client.Components;

/// <summary>
/// Blazor component for displaying and managing authenticated user information
/// Handles authentication state and access token retrieval
/// </summary>
public partial class UserDetailsComponent
{
    /// <summary>
    /// Gets or sets the cascading authentication state parameter
    /// </summary>
    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationState { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user's claims principal
    /// </summary>
    public ClaimsPrincipal? AuthenticatedUser { get; set; }
    /// <summary>
    /// Gets or sets the access token for the authenticated user
    /// </summary>
    public AccessToken? AccessToken { get; set; }

    /// <summary>
    /// Initializes the component and retrieves authentication state and access token
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when access token cannot be provisioned</exception>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (AuthenticationState is not null)
        {
            AuthenticationState state = await AuthenticationState;
            AccessTokenResult accessTokenResult = await AuthorizationService.RequestAccessToken();

            if (!accessTokenResult.TryGetToken(out AccessToken? token))
            {
                throw new InvalidOperationException(
                    "Failed to provision the access token.");
            }

            AccessToken = token;

            AuthenticatedUser = state.User;
        }
    }

    /// <summary>
    /// Extracts and deserializes claims from the access token
    /// Parses the JWT token payload to retrieve claim information
    /// </summary>
    /// <returns>A dictionary containing the token claims, or an empty dictionary if no token is available</returns>
    protected IDictionary<string, object>? GetAccessTokenClaims()
    {
        if (AccessToken is null)
        {
            return new Dictionary<string, object>();
        }

        // header.payload.signature
        string payload = AccessToken.Value.Split(".")[1];
        string base64Payload = payload.Replace('-', '+').Replace('_', '/')
            .PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

        IDictionary<string, object>? res = JsonSerializer.Deserialize<IDictionary<string, object>>(
           Convert.FromBase64String(base64Payload));

        return res;
    }
}