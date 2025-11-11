using Migration.SourceDb;
using Portal.Data;

namespace Migration.MigrationServices;

public class BaseMigrationClass
{
    protected readonly PrsDbContext _destinationContext;
    protected readonly SourceDBContext _sourceDBContext;

    public BaseMigrationClass(PrsDbContext destinationContext, SourceDBContext sourceDBContext)
    {
        _destinationContext = destinationContext;
        _sourceDBContext = sourceDBContext;
    }
}
