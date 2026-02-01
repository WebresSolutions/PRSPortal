using Portal.Data;
using Portal.Server.Controllers;
using Portal.Server.Helpers;
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

        Console.WriteLine("Skipping databases and identity services for IntegrationTests");

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

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        bool enableAuth = app.Configuration.GetValue<bool>("ApiSettings:AuthRequired");
        // Add API endpoints
        app.AddJobEndpoints(enableAuth);
        app.AddScheduleEndpoints(enableAuth);
        app.AddSettingEndpoints(enableAuth);
        app.AddCouncilEndpoints(enableAuth);
        app.AddContactEndpoints(enableAuth);
        app.TimeSheetendpoints(enableAuth);

        // Seed database with an initial client application and test user
        using (IServiceScope scope = app.Services.CreateScope())
        {
            IGraphService graphService = scope.ServiceProvider.GetService<IGraphService>() ?? throw new Exception("Graph service not registered.");
            PrsDbContext dbContext = scope.ServiceProvider.GetService<PrsDbContext>() ?? throw new Exception("Database not registered.");

            MigrateUsers.MigrateUsersFromAzure(dbContext, graphService);
        }

        app.Run();
    }
}