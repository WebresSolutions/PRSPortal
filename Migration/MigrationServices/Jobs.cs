using Migration.SourceDb;
using Portal.Data;

namespace Migration.MigrationServices;

internal class Jobs(PrsDbContext destinationContext, SourceDBContext sourceDBContext) : BaseMigrationClass(destinationContext, sourceDBContext)
{
    public void MigrateJobs()
    {
        // Get all of the jobs
    }
}
