using Portal.Server.Controllers;
using Scalar.AspNetCore;

namespace Portal.Server;

/// <summary>
/// Main entry point for the Casimo Server application
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
            {
                context.Request.Scheme = "https";
            }
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
        {
            app.UseHttpsRedirection();
        }

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors("CorsPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        // Add API endpoints
        app.AddJobEndpoints();
        app.AddScheduleEndpoints();

        app.Run();
    }
}