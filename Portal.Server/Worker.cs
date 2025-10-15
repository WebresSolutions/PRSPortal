using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Portal.Server.Data;
using Portal.Server.Options;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Portal.Server;

/// <summary>
/// Background service worker for initializing OpenIddict applications
/// Handles the creation of OAuth client applications during application startup
/// </summary>
public class Worker(IServiceProvider _serviceProvider) : IHostedService
{
    /// <summary>
    /// Starts the background service and initializes OpenIddict applications
    /// Creates the OAuth client application if it doesn't exist
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();

        AuthDBContext? context = scope.ServiceProvider.GetRequiredService<AuthDBContext>();
        IOptions<ClientOptions> clientOptions = scope.ServiceProvider.GetRequiredService<IOptions<ClientOptions>>();
        ClientOptions options = clientOptions.Value;

        _ = await context.Database.EnsureCreatedAsync(cancellationToken);
        IOpenIddictApplicationManager? manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        object? existingApplication = await manager.FindByClientIdAsync(options.ClientId, cancellationToken);

        if (existingApplication is null)
        {
            // Create new application
            _ = await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = options.ClientId,
                ConsentType = ConsentTypes.Implicit,
                DisplayName = options.DisplayName,
                ClientType = ClientTypes.Public,
                RedirectUris =
                {
                    new Uri(options.RedirectUri),
                    new Uri(options.RedirectUriHttp)
                },
                PostLogoutRedirectUris =
                {
                    new Uri(options.PostLogoutRedirectUri),
                    new Uri(options.PostLogoutRedirectUriHttp)
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.EndSession,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                },
            }, cancellationToken);
        }
        else
        {
            // Update existing application with new redirect URIs
            await manager.UpdateAsync(existingApplication, new OpenIddictApplicationDescriptor
            {
                ClientId = options.ClientId,
                ConsentType = ConsentTypes.Implicit,
                DisplayName = options.DisplayName,
                ClientType = ClientTypes.Public,
                RedirectUris =
                {
                    new Uri(options.RedirectUri),
                    new Uri(options.RedirectUriHttp)
                },
                PostLogoutRedirectUris =
                {
                    new Uri(options.PostLogoutRedirectUri),
                    new Uri(options.PostLogoutRedirectUriHttp)
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.EndSession,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                },
            }, cancellationToken);
        }

    }

    /// <summary>
    /// Stops the background service
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A completed task</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}