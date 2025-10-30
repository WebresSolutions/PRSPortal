﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        
        // Add Swagger/OpenAPI services for API debugging
        var enableSwagger = builder.Configuration.GetValue<bool>("ApiSettings:EnableSwagger");
        if (enableSwagger)
        {
            _ = builder.Services.AddEndpointsApiExplorer();
            _ = builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
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
                        "https://localhost:5001",      //API Standalone HTTPS
                        "https://casimo-portal-staging", // Staging environment
                        "https://casimo-portal.casimo.cloud" // Production environment
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()); // Allow credentials for authentication
        });

    }
}