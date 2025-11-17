using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Migration.Display;
using Migration.MigrationServices;
using Migration.SourceDb;
using Portal.Data;
using System.Collections.Frozen;
using Terminal.Gui;

internal class Program
{
    private static PrsDbContext? _destinationContext;
    private static SourceDBContext? _sourceContext;

    private static void Main(string[] args)
    {
        Application.Init();
        try
        {
            Application.Run(new GUI(InitMigration, StartMigration));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Application.Shutdown();
            throw;
        }
        finally
        {
            _sourceContext?.Dispose();
            _destinationContext?.Dispose();
        }
    }

    private static void StartMigration(Action<MigrationProgress> progressCallback)
    {
        progressCallback(new MigrationProgress
        {
            CurrentStep = "Database Reset",
            CurrentItem = "Database schema reset complete",
            CurrentItemIndex = 1,
            TotalItems = 1
        });

        Users userMigration = new(_destinationContext!, _sourceContext!);
        FrozenDictionary<int, int> users = userMigration.MigrateUsers(progressCallback);
        try
        {

            MigrationService migration = new(_destinationContext!, _sourceContext!, users);
            migration.MigrateContacts(progressCallback);
            migration.MigrateCouncils(progressCallback);
            migration.MigrateJobs(progressCallback);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static bool InitMigration(bool resetDatabase)
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);
        _ = builder.Build();

        IConfiguration config = builder.Build();

        string? destinationConnection = config.GetConnectionString("PSQLConnection");

        DbContextOptions<PrsDbContext> dbContextOptions = new DbContextOptionsBuilder<PrsDbContext>()
            .UseNpgsql(destinationConnection)
            .Options;

        _destinationContext = new(dbContextOptions);
        _sourceContext = new();

        if (_destinationContext is null || _sourceContext is null)
            throw new Exception("Context found to be null.");

        if (resetDatabase)
        {
            // Get the actual source directory, not the bin directory
            string solutionDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory()));
            string sqlPath = Path.Combine(solutionDir, "database_schema.sql");

            // Verify the file exists and show the path
            if (!File.Exists(sqlPath))
            {
                throw new FileNotFoundException($"SQL file not found at: {sqlPath}");
            }

            string resetQuery = File.ReadAllText(sqlPath);
            _destinationContext.Database.ExecuteSqlRaw(resetQuery);
        }

        // Testing connection 
        return !_destinationContext.Database.CanConnect()
            ? throw new Exception("failed to connect to the database")
            : !_sourceContext.Database.CanConnect() ? throw new Exception("failed to connect to the database") : true;
    }
}