using Portal.Server.Endpoints;
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

        builder.AddDatabases();
        builder.AddOtherServices();

        WebApplication? app = builder.Build();

        _ = app.Use((context, next) =>
        {
            if (context.Request.Scheme != "https")
            {
                context.Request.Scheme = "https";
            }
            return next(context);
        });
        _ = app.UseForwardedHeaders();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            _ = app.UseDeveloperExceptionPage();
            //_ = app.MapOpenApi();
        }
        else
        {
            _ = app.UseExceptionHandler("/Error");
            _ = app.UseHsts();
        }

        // Only use HTTPS redirection in development or when not behind a proxy
        if (app.Environment.IsDevelopment())
        {
            _ = app.MapOpenApi();
            _ = app.MapScalarApiReference(options =>
            {
                options.Title = "PRS API";
                options.Theme = ScalarTheme.Purple;
            });
        }

        _ = app.UseStaticFiles();
        _ = app.UseRouting();
        _ = app.UseCors("CorsPolicy");

        _ = app.UseAuthentication();
        _ = app.UseAuthorization();

        _ = app.MapControllers();
        _ = app.MapFallbackToFile("index.html");

        _ = app.MapGet(app.Environment.ApplicationName, () => "Portal Server is running.");
        app.MapWeatherForecastEndpoints();

        app.Run();
    }
}