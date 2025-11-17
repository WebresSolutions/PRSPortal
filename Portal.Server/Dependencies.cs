using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Portal.Data;

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
        _ = builder.Services.AddDbContextPool<PrsDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("PrsConnection")));
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

        // Add Swagger/OpenAPI services for API debugging
        bool enableSwagger = builder.Configuration.GetValue<bool>("ApiSettings:EnableSwagger");
        if (enableSwagger)
        {
            _ = builder.Services.AddEndpointsApiExplorer();
            _ = builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Portal API",
                    Version = "v1",
                    Description = "API for Portal application debugging"
                });
            });
        }
        _ = builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder.WithOrigins(
                        "http://localhost:44310",      //Local port
                        "https://localhost:44310",
                        "https://localhost:44331",
                        "https://localhost:3000",
                        "http://localhost:5000",       //API Standalone HTTP
                        "https://localhost:5001"     //API Standalone HTTPS
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()); // Allow credentials for authentication
        });

    }
}