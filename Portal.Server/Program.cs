using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Portal.Data;
using Portal.Server.Controllers;
using Portal.Server.Endpoints;
using Portal.Server.Middleware;
using Portal.Server.Services.Interfaces;
using Scalar.AspNetCore;

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

        if (app.Environment.IsEnvironment("Testing"))
            app.UseMiddleware<TestUserContextMiddleware>();
        else
            app.UseMiddleware<CustomMiddleware>();

        app.MapRazorPages();
        app.MapControllers();

        bool enableAuth = app.Configuration.GetValue<bool>("ApiSettings:AuthRequired");

        app.AddIntegrationEndpoints();
        app.AddJobEndpoints(enableAuth);
        app.AddScheduleEndpoints(enableAuth);
        app.AddSettingEndpoints(enableAuth);
        app.AddCouncilEndpoints(enableAuth);
        app.AddContactEndpoints(enableAuth);
        app.AddTimeSheetendpoints(enableAuth);
        app.AddTypesEndpoints(enableAuth);
        app.AddUserEndpoints(enableAuth);
        app.AddFileEndpoints(enableAuth);

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
            IGraphService graphService = scope.ServiceProvider.GetService<IGraphService>() ?? throw new Exception("Graph service not registered.");
            PrsDbContext dbContext = scope.ServiceProvider.GetService<PrsDbContext>() ?? throw new Exception("Database not registered.");

            //MigrateUsers.MigrateUsersFromAzure(dbContext, graphService);
        }

        app.Run();
    }
}