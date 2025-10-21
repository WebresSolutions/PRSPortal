using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

namespace Portal.Client;

public class Program
{
    private static async Task Main(string[] args)
    {
        WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");

        string httpClient = builder.Configuration.GetValue<string>("HttpClient")
            ?? throw new Exception("Failed to load the http client settings");

        // 1. Register the HttpClient with the AuthorizationMessageHandler
        _ = builder.Services.AddHttpClient(httpClient)
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

        // 2. Register the authorized HttpClient as the default
        _ = builder.Services.AddScoped(provider =>
        {
            IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();
            return factory.CreateClient(httpClient); // This is the authorized client
        });

        Console.WriteLine("Configuring MSAL Authentication");
        string? clientId = builder.Configuration.GetValue<string>("AzureAd:ClientId");
        string? authority = builder.Configuration.GetValue<string>("AzureAd:Authority");

        Console.WriteLine($"ClientId: {clientId}");
        Console.WriteLine($"authority: {authority}");

        _ = builder.Services.AddMsalAuthentication(options =>
        {
            builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
            options.ProviderOptions.LoginMode = "redirect";
            options.ProviderOptions.DefaultAccessTokenScopes.Add("profile");
            options.ProviderOptions.DefaultAccessTokenScopes.Add("offline_access");
            options.ProviderOptions.DefaultAccessTokenScopes.Add($"api://{builder.Configuration.GetValue<string>("AzureAd:ClientId")}/read_as_user");
        });

        //builder.Services.AddScoped<IApiService, ApiService>();
        _ = builder.Services.AddMudServices(config =>
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
    }
}