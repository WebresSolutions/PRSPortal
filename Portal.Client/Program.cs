using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Portal.Client.Services.Instances;
using Portal.Client.Services.Interfaces;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace Portal.Client;

/// <summary>
/// Main entry point for the Portal Client Blazor WebAssembly application
/// Configures authentication, HTTP clients, and application services
/// </summary>
public class Program
{
    /// <summary>
    /// Entry point for the application
    /// Initializes the Blazor WebAssembly host with authentication and service configuration
    /// </summary>
    /// <param name="args">Command line arguments</param>
    private static async Task Main(string[] args)
    {
        WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        string clientId = builder.Configuration.GetValue<string>("AzureAd:ClientId") ?? throw new Exception("Azure Client Id is null ");
        string? authority = builder.Configuration.GetValue<string>("AzureAd:Authority");
        builder.Services.AddMsalAuthentication(options =>
        {
            builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
            options.ProviderOptions.LoginMode = "redirect";
            options.ProviderOptions.DefaultAccessTokenScopes.Add("profile");
            options.ProviderOptions.DefaultAccessTokenScopes.Add("offline_access");
            options.ProviderOptions.DefaultAccessTokenScopes.Add($"api://{clientId}/read_as_user");
        });

        string httpClient = builder.Configuration.GetValue<string>("HttpClient")
            ?? throw new Exception("Failed to load the http client settings");

        string? baseUri = builder.Configuration.GetValue<string>("APIUrl");
        if (string.IsNullOrEmpty(baseUri))
            baseUri = builder.HostEnvironment.BaseAddress;

        // 1. Register the HttpClient with the AuthorizationMessageHandler
        // BaseAddressAuthorizationMessageHandler only works for same-origin requests.
        // Since API is on a different URL (https://localhost:5001), we need AuthorizationMessageHandler
        // with explicit configuration for cross-origin requests.
        builder.Services.AddHttpClient(httpClient)
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(baseUri))
            .AddHttpMessageHandler(sp =>
            {
                IAccessTokenProvider tokenProvider = sp.GetRequiredService<IAccessTokenProvider>();
                NavigationManager navigationManager = sp.GetRequiredService<NavigationManager>();
                string apiScope = $"api://{clientId}/read_as_user";

                AuthorizationMessageHandler handler = new(
                    tokenProvider,
                    navigationManager);

                handler.ConfigureHandler(
                    authorizedUrls: [baseUri],
                    scopes: [apiScope]);

                return handler;
            });

        // 2. Register the authorized HttpClient as the default
        builder.Services.AddScoped(provider =>
        {
            IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();
            return factory.CreateClient(httpClient); // This is the authorized client
        });

        Console.WriteLine("Configuring MSAL Authentication");

        Console.WriteLine($"ClientId: {clientId}");
        Console.WriteLine($"authority: {authority}");
        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;

            config.SnackbarConfiguration.PreventDuplicates = true;
            config.SnackbarConfiguration.NewestOnTop = false;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 8000;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
        });
        builder.Services.AddSingleton<IApiService, ApiService>();
        // The service holds stateful information about the current user session.
        builder.Services.AddSingleton<SessionStorage>();
        builder.Services.AddHotKeys2();

        WebAssemblyHost host = builder.Build();
        await host.RunAsync();
    }
}