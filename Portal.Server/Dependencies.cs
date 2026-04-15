using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Portal.Data;
using Portal.Server.Options;
using Portal.Server.Services.Instances;
using Portal.Server.Services.Instances.UtilityServices;
using Portal.Server.Services.Interfaces;
using Portal.Server.Services.Interfaces.UtilityServices;
using Portal.Server.Services.Mocks;
using Quartz;
using Smtp2Go.Api;
using Xero.NetStandard.OAuth2.Api;

namespace Portal.Server;

/// <summary>
/// Static class containing dependency injection configuration methods
/// Provides extension methods for configuring services in the application
/// </summary>
public static class Dependencies
{
    /// <summary>
    /// Configures database connections for the application
    /// Sets up the PostgreSQL database context with connection string from configuration
    /// </summary>
    /// <param name="builder">The web application builder to configure</param>
    public static void AddDatabases(this WebApplicationBuilder builder)
    {
        Console.WriteLine("Using ENV: " + builder.Environment.EnvironmentName);
        builder.Services.AddDbContext<PrsDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("PrsConnection"), x => x.UseNetTopologySuite()));
    }

    /// <summary>
    /// Configures additional services including database context, business services, and CORS
    /// Sets up the application's business logic infrastructure
    /// </summary>
    /// <param name="builder">The web application builder to configure</param>
    public static void AddServices(this WebApplicationBuilder builder)
    {
        // Add the custom services
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        // Configure JWT validation parameters
        builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters.ValidateIssuer = true;
            options.TokenValidationParameters.ValidateAudience = true;
            options.TokenValidationParameters.ValidateLifetime = true;
        });

        builder.Services.AddAuthorization();
        builder.Services.Configure<XeroOptions>(builder.Configuration.GetSection("XeroSettings"));
        builder.Services.Configure<SharePointOptions>(builder.Configuration.GetSection("SharepointOptions"));
        builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailOptions"));

        builder.WebHost.UseStaticWebAssets();
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        builder.Services.AddQuartz();
        // Add Quartz.NET as a hosted service
        builder.Services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
        // Injects in memory cache
        builder.Services.AddMemoryCache();

        #region Singleton and Transient Services.
        // This is the acconting api from xero
        builder.Services.AddTransient<ISharePointService>(provider =>
        {
            ILogger<SharePointService> logger = provider.GetService<ILogger<SharePointService>>()
            ?? throw new Exception("Failed to find the logger");
            SharePointOptions options = provider.GetRequiredService<IOptions<SharePointOptions>>().Value;
            if (options.UseMock)
                return new SharePointServiceMock(options, logger);
            else
                return new SharePointService(options, logger);
        });
        builder.Services.AddSingleton<IApiService, Smtp2GoApiService>(provider =>
        {
            EmailOptions options = provider.GetRequiredService<IOptions<EmailOptions>>().Value;
            return new Smtp2GoApiService(options.ApiKey);
        });
        builder.Services.AddSingleton<IAccountingApi, AccountingApi>();
        builder.Services.AddSingleton<IPayrollAuApi, PayrollAuApi>();
        builder.Services.AddTransient<IRazorViewRenderer, RazorViewRenderer>();
        #endregion

        #region Scoped Services.
        builder.Services.AddScoped<IFileService, FileService>();
        builder.Services.AddScoped<IPdfGenerationService, PdfGenerationService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IXeroIntegrationService, XeroIntegrationService>();
        builder.Services.AddScoped<IJobService, JobService>();
        builder.Services.AddScoped<IScheduleService, ScheduleService>();
        builder.Services.AddScoped<ISettingService, SettingService>();
        builder.Services.AddScoped<ICouncilService, CouncilService>();
        builder.Services.AddScoped<IContactService, ContactService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ITimeSheetService, TimeSheetService>();
        builder.Services.AddScoped<ITypesService, TypesService>();
        builder.Services.AddScoped<IQuoteService, QuoteService>();
        #endregion

        // Add Swagger/OpenAPI services for API debugging
        if (builder.Configuration.GetValue<bool>("ApiSettings:EnableSwagger"))
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen(c =>
            {
            });
        }
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:5000",
                        "https://localhost:5001",
                        "http://localhost:44310",
                        "https://localhost:44310",
                        "http://localhost:5555",
                        "http://localhost:7357",
                        "https://1b8q9wlp-5001.aue.devtunnels.ms"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

    }
}