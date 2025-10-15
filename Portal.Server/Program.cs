using Portal.Server.Models;

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
        _ = builder.WebHost.UseUrls();

        Console.WriteLine($"Environment Name: '{builder.Environment.EnvironmentName}'");

        Console.WriteLine("Adding databases and identity services");
        builder.AddDatabases();
        builder.AddIdentityServices();

        Console.WriteLine("Skipping databases and identity services for IntegrationTests");

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
            app.UseWebAssemblyDebugging();
            //_ = app.MapOpenApi();
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

        _ = app.MapGroup("/api/account").MapIdentityApi<ApplicationUser>();


        //if (app.Environment.EnvironmentName is not StaticStrings.IntegrationEnvName)
        //{
        //    // Validata the connection
        //    using (IServiceScope scope = app.Services.CreateScope())
        //    {
        //        AuthDBContext? authDB = scope.ServiceProvider.GetRequiredService<AuthDBContext>();
        //        CasimoDbContext? casimoDB = scope.ServiceProvider.GetRequiredService<CasimoDbContext>();

        //        if (!casimoDB.IsConnected())
        //        {
        //            throw new Exception("Connection to the Casimo DB failed");
        //        }
        //        if (!authDB.IsConnected())
        //        {
        //            throw new Exception("Connection to the Identity DB failed");
        //        }
        //    }

        //    // Seed database with an initial client application and test user
        //    using (IServiceScope scope = app.Services.CreateScope())
        //    {
        //        AuthDBContext? authDB = scope.ServiceProvider.GetRequiredService<AuthDBContext>();
        //        CasimoDbContext? casimoDB = scope.ServiceProvider.GetRequiredService<CasimoDbContext>();
        //        RoleManager<IdentityRole>? roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        //        UserManager<ApplicationUser>? userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        //        // Call the database seeder (made this awaitable)
        //        await Seeder.SeedDatabase(roleManager, userManager, casimoDB);
        //    }
        //}

        app.Run();
    }
}