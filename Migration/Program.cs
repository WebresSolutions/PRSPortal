using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Migration.MigrationServices;
using Migration.SourceDb;
using Portal.Data;
using System.Collections.Frozen;

internal class Program
{
    private static PrsDbContext? _destinationContext;
    private static SourceDBContext? _sourceContext;

    private static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Connecting to databases and loading the configuration.");
            bool initSuccess = InitMigration();
            if (!initSuccess || _destinationContext is null || _sourceContext is null)
            {
                Console.WriteLine("Context was null. Cannot continue.");
                throw new Exception("Context found to be null.");
            }

            Users userMigration = new(_destinationContext, _sourceContext);
            FrozenDictionary<int, int> users = userMigration.MigrateUsers();
            Jobs jobMigration = new(_destinationContext, _sourceContext, users);
            jobMigration.MigrateJobs();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        finally
        {
            _sourceContext?.Dispose();
            _destinationContext?.Dispose();
        }
    }

    private static bool InitMigration()
    {
        Console.WriteLine("Starting database migration ");
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

        // Testing connection 
        return !_destinationContext.Database.CanConnect()
            ? throw new Exception("failed to connect to the database")
            : !_sourceContext.Database.CanConnect() ? throw new Exception("failed to connect to the database") : true;
    }
}