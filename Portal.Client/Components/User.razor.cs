using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Security.Claims;
using System.Text.Json;

namespace Portal.Client.Components;

public partial class User
{
    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationState { get; set; }

    public ClaimsPrincipal? AuthenticatedUser { get; set; }
    public AccessToken? AccessToken { get; set; }

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