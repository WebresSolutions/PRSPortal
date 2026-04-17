using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using Portal.Data;
using Portal.Server.Controllers;
using Portal.Server.Endpoints;
using Portal.Server.Middleware;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;

namespace Portal.Server;

/// <summary>
/// Main entry point for the PRS Portal Server application
/// Configures and runs the ASP.NET Core web application with authentication, database, and API endpoints
/// </summary>

public class Program
{
    private static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRateLimiter(options =>
        {
            options.AddSlidingWindowLimiter("sliding-policy", opt =>
            {
                opt.PermitLimit = 100;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.SegmentsPerWindow = 4; // 15-second segments
                opt.QueueLimit = 2;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });

        Console.WriteLine($"Environment Name: '{builder.Environment.EnvironmentName}'");

        Console.WriteLine("Adding databases and identity services");
        builder.AddDatabases();

        Console.WriteLine("Adding application services");
        builder.AddServices();

        WebApplication? app = builder.Build();

        app.Use((context, next) =>
        {
            if (context.Request.Scheme != "https")
                context.Request.Scheme = "https";

            return next(context);
        });
        app.UseForwardedHeaders();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();

            // Enable Swagger/OpenAPI for API debugging
            bool enableSwagger = app.Configuration.GetValue<bool>("ApiSettings:EnableSwagger");

            if (enableSwagger)
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Portal API v1");
                    c.RoutePrefix = "swagger";
                });
            }
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        // Only use HTTPS redirection in development or when not behind a proxy
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors("CorsPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        // Apply to middleware
        app.UseRateLimiter();
        if (app.Environment.IsEnvironment("Testing"))
            app.UseMiddleware<TestUserContextMiddleware>();
        else
            app.UseMiddleware<PRSMiddleware>();

        app.MapRazorPages();
        app.MapControllers();

        bool enableAuth = app.Configuration.GetValue<bool>("ApiSettings:AuthRequired");

        app.AddIntegrationEndpoints("Xero")
            .AddJobEndpoints("Jobs", enableAuth)
            .AddScheduleEndpoints("Scheduling", enableAuth)
            .AddSettingEndpoints("Settings", enableAuth)
            .AddCouncilEndpoints("Councils", enableAuth)
            .AddContactEndpoints("Contacts", enableAuth)
            .AddTimeSheetendpoints("TimeSheets", enableAuth)
            .AddTypesEndpoints("Types", enableAuth)
            .AddUserEndpoints("Users", enableAuth)
            .AddFileEndpoints("Files", enableAuth)
            .AddQuoteEndpoints("Quotes", enableAuth)
            .AddClientEndpoints();

        // Only serve SPA index.html for non-API paths so /api/* returns JSON from endpoints, not HTML
        app.MapFallback(async (HttpContext context) =>
        {
            if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Not Found");
                return;
            }
            IWebHostEnvironment env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
            IFileInfo? file = env.WebRootFileProvider.GetFileInfo("index.html");
            if (file.Exists)
            {
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync(file);
            }
            else
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Not Found");
            }
        });

        // Seed database with an initial client application and test user
        using (IServiceScope scope = app.Services.CreateScope())
        {
            PrsDbContext dbContext = scope.ServiceProvider.GetService<PrsDbContext>() ?? throw new Exception("Database not registered.");

            //MigrateUsers.MigrateUsersFromAzure(dbContext, graphService);
        }

        app.Run();
    }
}