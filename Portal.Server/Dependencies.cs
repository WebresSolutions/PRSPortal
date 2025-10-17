using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portal.Server.Data;
using Portal.Server.Models;
using Portal.Server.Options;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;

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
        string authConnectionString = builder.Configuration.GetConnectionString("AuthConnection")
            ?? throw new Exception("Connection string 'AuthConnection' not found.");

        // 1. Automatically detect the MySQL server version from the connection string
        ServerVersion serverVersion = ServerVersion.AutoDetect(authConnectionString);

        _ = builder.Services.AddDbContext<AuthDBContext>(options =>
        {
            _ = options.UseMySql(authConnectionString, serverVersion);
            _ = options.UseOpenIddict();
        });

        Console.WriteLine($"Auth Connection: {authConnectionString}");
    }

    /// <summary>
    /// Configures identity services including OpenIddict, Entity Framework, and authentication
    /// Sets up the complete authentication and authorization infrastructure
    /// </summary>
    /// <param name="builder">The web application builder to configure</param>
    public static void AddIdentityServices(this WebApplicationBuilder builder)
    {
        _ = builder.Services.Configure<ClientOptions>(builder.Configuration.GetSection("ClientOptions"));

        // Register the Identity services
        _ = builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<AuthDBContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

        _ = builder.Services.ConfigureApplicationCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromHours(24);
            options.SlidingExpiration = false;
        });

        // OpenIddict offers native integration with Quartz.NET for scheduled tasks
        _ = builder.Services.AddQuartz(options =>
        {
            options.UseSimpleTypeLoader();
            options.UseInMemoryStore();
        });

        // Register the Quartz.NET service
        _ = builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        // Check if we're in a proxy environment (staging/production behind Cloudflare)
        bool isBehindProxy = builder.Environment.IsStaging() || builder.Environment.IsProduction();

        _ = builder.Services.AddOpenIddict()
            // Register the OpenIddict core components
            .AddCore(options =>
            {
                // Configure OpenIddict to use the Entity Framework Core stores and models
                _ = options.UseEntityFrameworkCore()
                       .UseDbContext<AuthDBContext>();

                // Enable Quartz.NET integration
                _ = options.UseQuartz();
            })
            // Register the OpenIddict client components
            .AddClient(options =>
            {
                // Allow authorization code flow
                _ = options.AllowAuthorizationCodeFlow();

                // Register the signing and encryption credentials
                _ = options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                // Register the ASP.NET Core host and configure options
                _ = options.UseAspNetCore()
                       .EnableStatusCodePagesIntegration()
                       .EnableRedirectionEndpointPassthrough();

                // Register the System.Net.Http integration
                _ = options.UseSystemNetHttp()
                       .SetProductInformation(typeof(Program).Assembly);

                // Configure redirection endpoint manually (required for authorization code flow)
                // This replaces UseWebProviders() when you don't want external providers
                _ = options.SetRedirectionEndpointUris("/callback/login/");
            })
            // Register the OpenIddict server components
            .AddServer(options =>
            {
                // Enable endpoints
                _ = options.SetAuthorizationEndpointUris("connect/authorize")
                       .SetEndSessionEndpointUris("connect/logout")
                       .SetTokenEndpointUris("connect/token")
                       .SetUserInfoEndpointUris("connect/userinfo");

                // Register scopes
                _ = options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                // Configure flows
                _ = options.AllowAuthorizationCodeFlow()
                       .AllowRefreshTokenFlow();

                // Register credentials
                _ = options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                // Configure ASP.NET Core options
                _ = options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableEndSessionEndpointPassthrough()
                       .EnableStatusCodePagesIntegration()
                       .EnableTokenEndpointPassthrough();
            })
            // Register the OpenIddict validation components
            .AddValidation(options =>
            {
                _ = options.UseLocalServer();
                _ = options.UseAspNetCore();
            });
        _ = builder.Services.AddHostedService<Worker>();

        // Add some policies
        //builder.Services.AddAuthorizationBuilder()
        //    // Add some policies
        //    .AddPolicy(PolicyConstants.AdminOnly, policy => policy.RequireRole(RoleConstants.AdminUser))
        //    // Add some policies
        //    .AddPolicy(PolicyConstants.FullUserOnly, policy => policy.RequireRole(RoleConstants.AdminUser, RoleConstants.FullUser))
        //    // Add some policies
        //    .AddPolicy(PolicyConstants.TemporaryUserOnly, policy => policy.RequireRole(RoleConstants.TemporaryUser));

    }

    /// <summary>
    /// Configures additional services including database context, business services, and CORS
    /// Sets up the application's business logic infrastructure
    /// </summary>
    /// <param name="builder">The web application builder to configure</param>
    public static void AddOtherServices(this WebApplicationBuilder builder)
    {
        _ = builder.WebHost.UseStaticWebAssets();
        // Add the custom services

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