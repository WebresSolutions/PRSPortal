using Migration.SourceDb;
using Portal.Data;
using Portal.Data.Models;
using System.Collections.Frozen;

namespace Migration.MigrationServices;

internal class Jobs(PrsDbContext destinationContext, SourceDBContext sourceDBContext, FrozenDictionary<int, int> users) : BaseMigrationClass(destinationContext, sourceDBContext)
{

    public void MigrateJobs()
    {
        // Get all of the jobs
        Migration.SourceDb.Job[] jobs = _sourceDBContext.Jobs.ToArray();
        Console.WriteLine($"Job Count: {jobs.Length}");

        foreach (SourceDb.Job job in jobs)
        {
            // Find an address
            Address address = Helpers.FindOrCreateAddress(_destinationContext, job.State, job.Address, job.Suburb ?? "", job.Postcode);

            Portal.Data.Models.Job newJob = new()
            {

            };
        }

        _destinationContext.SaveChanges();
    }
}
