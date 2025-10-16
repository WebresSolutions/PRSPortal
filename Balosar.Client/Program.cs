using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Portal.Client;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

string httpClient = builder.Configuration.GetValue<string>("HttpClient")
    ?? throw new Exception("Failed to load the http client settings");

builder.Services.AddHttpClient(httpClient)
    .ConfigureHttpClient(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests to the server project.
builder.Services.AddScoped(provider =>
{
    IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient(httpClient);
});

builder.Services.AddOidcAuthentication(options =>
{
    string baseAddress = builder.HostEnvironment.BaseAddress;
    options.ProviderOptions.ClientId = builder.Configuration.GetValue<string>("ClientId");
    options.ProviderOptions.Authority = baseAddress;
    options.ProviderOptions.ResponseType = "code";

    // Note: response_mode=fragment is the best option for a SPA. Unfortunately, the Blazor WASM
    // authentication stack is impacted by a bug that prevents it from correctly extracting
    // authorization error responses (e.g error=access_denied responses) from the URL fragment.
    // For more information about this bug, visit https://github.com/dotnet/aspnetcore/issues/28344.
    //
    options.ProviderOptions.ResponseMode = "query";

    // Use dynamic base URL for authentication paths instead of hardcoded configuration
    options.AuthenticationPaths.RemoteRegisterPath = $"{baseAddress}Identity/Account/Register";

    // Add the "roles" (OpenIddictConstants.Scopes.Roles) scope and the "role" (OpenIddictConstants.Claims.Role) claim
    // (the same ones used in the Startup class of the Server) in order for the roles to be validated.
    // See the Counter component for an example of how to use the Authorize attribute with roles
    options.ProviderOptions.DefaultScopes.Add("roles");
    options.UserOptions.RoleClaim = "role";
});

//builder.Services.AddScoped<IApiService, ApiService>();
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


WebAssemblyHost host = builder.Build();
await host.RunAsync();
