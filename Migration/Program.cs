using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Migration;
using Migration.Display;
using Migration.MigrationServices;
using Migration.SourceDb;
using System.Collections.Frozen;
using Terminal.Gui;

/// <summary>
/// Main program class for database migration tool
/// Handles migration from MySQL source database to PostgreSQL destination database
/// </summary>
internal class Program
{
    /// <summary>
    /// The destination PostgreSQL database context
    /// </summary>
    private static PrsDbContext? _destinationContext;
    /// <summary>
    /// The source MySQL database context
    /// </summary>
    private static SourceDBContext? _sourceContext;

    /// <summary>
    /// Entry point for the migration application
    /// Initializes the GUI and starts the migration process
    /// </summary>
    /// <param name="args">Command line arguments</param>
    private static void Main(string[] args)
    {
        Application.Init();
        try
        {
            GUI gui = new(InitMigration, StartMigration);
            Application.Run(gui);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            if (Application.Top is not null)
                Application.RequestStop();

            throw;
        }
        finally
        {
            _sourceContext?.Dispose();
            _destinationContext?.Dispose();
        }
    }

    /// <summary>
    /// Starts the migration process with progress reporting
    /// Executes all migration steps in sequence
    /// </summary>
    /// <param name="progressCallback">Callback function for reporting migration progress</param>
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
            migration.SeedBaseData();
            migration.MigrateContacts(progressCallback);
            migration.MigrateCouncils(progressCallback);
            migration.MigrateJobs(progressCallback);
            migration.MigrateSchedule(progressCallback);
            migration.MigrateUserJobs(progressCallback);
            migration.MigratateJobsSubObjects(progressCallback);
            migration.MigarateTasks(progressCallback);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Initializes the database connections and optionally resets the destination database
    /// </summary>
    /// <param name="resetDatabase">True to reset the destination database schema before migration</param>
    /// <returns>True if initialization was successful, otherwise throws an exception</returns>
    /// <exception cref="Exception">Thrown when database contexts are null or connection fails</exception>
    /// <exception cref="FileNotFoundException">Thrown when the database schema SQL file is not found</exception>
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
        // This connection is hard wired into the context
        _sourceContext = new();

        if (_destinationContext is null || _sourceContext is null)
            throw new Exception("Context found to be null.");

        if (resetDatabase)
        {
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