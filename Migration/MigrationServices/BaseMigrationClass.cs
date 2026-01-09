using Migration.SourceDb;

namespace Migration.MigrationServices;

/// <summary>
/// Base class for migration service implementations
/// Provides common database context access for migration operations
/// </summary>
public class BaseMigrationClass
{
    /// <summary>
    /// The destination PostgreSQL database context
    /// </summary>
    protected readonly PrsDbContext _destinationContext;
    /// <summary>
    /// The source MySQL database context
    /// </summary>
    protected readonly SourceDBContext _sourceDBContext;

    /// <summary>
    /// Initializes a new instance of the BaseMigrationClass
    /// </summary>
    /// <param name="destinationContext">The destination database context</param>
    /// <param name="sourceDBContext">The source database context</param>
    public BaseMigrationClass(PrsDbContext destinationContext, SourceDBContext sourceDBContext)
    {
        _destinationContext = destinationContext;
        _sourceDBContext = sourceDBContext;
    }
}
