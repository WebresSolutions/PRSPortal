using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.Identity.Web;


namespace Portal.Server;

/// <summary>
/// Static class containing dependency injection configuration methods
/// Provides extension methods for configuring services in the application
/// </summary>
public static class Dependencies
{

    public static void AddDatabases(this WebApplicationBuilder builder)
    {
        Console.WriteLine("Using ENV: " + builder.Environment.EnvironmentName);
    }

    /// <summary>
    /// Configures additional services including database context, business services, and CORS
    /// Sets up the application's business logic infrastructure
    /// </summary>
    /// <param name="builder">The web application builder to configure</param>
    public static void AddOtherServices(this WebApplicationBuilder builder)
    {
        // Add the custom services
        _ = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        _ = builder.WebHost.UseStaticWebAssets();
        _ = builder.Services.AddControllersWithViews();
        _ = builder.Services.AddRazorPages();
        _ = builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder.WithOrigins(
                        "http://localhost:44310",      //Local port
                        "https://localhost:44310",
                        "https://casimo-portal-staging", // Staging environment
                        "https://casimo-portal.casimo.cloud" // Production environment
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()); // Allow credentials for authentication
        });

    }
}