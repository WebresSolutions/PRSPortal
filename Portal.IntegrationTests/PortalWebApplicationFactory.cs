using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Portal.Data;
using Portal.Server;
using Testcontainers.PostgreSql;

namespace Portal.IntegrationTests;

/// <summary>
/// Web application factory for integration tests. Uses a real PostgreSQL container;
/// start the container by calling <see cref="InitializeAsync"/> (e.g. from a fixture) before using the factory.
/// </summary>
public sealed class PortalWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // PostGIS image required because PrsDbContext uses postgis and pg_trgm extensions
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgis/postgis:16-3.5-alpine")
        .WithDatabase("prs_test")
        .WithUsername("postgres")
        .WithPassword("145269")
        .Build();

    /// <summary>
    /// Call this before using the factory (e.g. in a fixture's IAsyncLifetime.InitializeAsync).
    /// Starts the PostgreSQL container so the connection string is available when the host is built.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        ;
    }

    public new async Task DisposeAsync() => await _postgres.DisposeAsync();

    public string ConnectionString => _postgres.GetConnectionString();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Configure test settings: use container connection string and disable auth
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PrsConnection"] = ConnectionString,
                ["ApiSettings:AuthRequired"] = "false",
                ["ApiSettings:EnableSwagger"] = "false",
                ["AzureAd:TenantId"] = "3f0723ef-ae58-4ab8-8b75-e4a6e8eeee31",
                ["AzureAd:ClientId"] = "26b1d3cf-bc97-4c99-a5ec-bdda667e9fb9",
                // TestUserContextMiddleware uses these so HttpContext.Items["UserId"] and ["UserEmail"] are set (matches seed user)
                ["Testing:TestUserId"] = "1",
                ["Testing:TestUserEmail"] = "testuser@example.com",
            });
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Ensures the database schema exists (EnsureCreated). Call once after the host is built and container is running.
    /// </summary>
    public async Task EnsureDatabaseSchemaAsync()
    {
        using IServiceScope scope = Server.Services.CreateScope();
        PrsDbContext db = scope.ServiceProvider.GetRequiredService<PrsDbContext>();
        await db.Database.EnsureCreatedAsync();
        string solutionDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory()));
        string seedSQL = Path.Combine(solutionDir, "seed.sql");
        // Verify the file exists and show the path
        if (!File.Exists(seedSQL))
        {
            throw new FileNotFoundException($"SQL file not found at: {seedSQL}");
        }

        string seedQuery = File.ReadAllText(seedSQL);
        // Seed the database
        db.Database.ExecuteSqlRaw(seedQuery);
    }
}
