using Portal.Server.Controllers;

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
            app.UseWebAssemblyDebugging();

            // Enable Swagger/OpenAPI for API debugging
            bool enableSwagger = app.Configuration.GetValue<bool>("ApiSettings:EnableSwagger");
            if (enableSwagger)
            {
                _ = app.UseSwagger();
                _ = app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Portal API v1");
                    c.RoutePrefix = "swagger";
                });
            }
        }
        else
        {
            _ = app.UseExceptionHandler("/Error");
            _ = app.UseHsts();
        }

        // Only use HTTPS redirection in development or when not behind a proxy
        if (!app.Environment.IsDevelopment())
        {
            _ = app.UseHttpsRedirection();
        }

        _ = app.UseBlazorFrameworkFiles();
        _ = app.UseStaticFiles();
        _ = app.UseRouting();
        _ = app.UseCors("CorsPolicy");

        _ = app.UseAuthentication();
        _ = app.UseAuthorization();

        _ = app.MapRazorPages();
        _ = app.MapControllers();
        _ = app.MapFallbackToFile("index.html");
        app.AddJobEndpoints();

        app.Run();
    }
}