using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Portal.Data;
using Portal.Server.Services.Instances;
using Portal.Server.Services.Interfaces;

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
        builder.Services.AddDbContext<PrsDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("PrsConnection")));
    }

    /// <summary>
    /// Configures additional services including database context, business services, and CORS
    /// Sets up the application's business logic infrastructure
    /// </summary>
    /// <param name="builder">The web application builder to configure</param>
    public static void AddServices(this WebApplicationBuilder builder)
    {
        // Add the custom services
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        builder.WebHost.UseStaticWebAssets();
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.Services.AddScoped<IJobService, JobService>();
        builder.Services.AddScoped<IScheduleService, ScheduleService>();

        // Add Swagger/OpenAPI services for API debugging
        bool enableSwagger = builder.Configuration.GetValue<bool>("ApiSettings:EnableSwagger");
        if (enableSwagger)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Portal API",
                    Version = "v1",
                    Description = "API for Portal application debugging"
                });
            });
        }
        builder.Services.AddCors(options =>
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