using Portal.Data;
using Portal.Server.Services.Interfaces;

namespace Portal.Server.Helpers;

public static class MigrateUsers
{
    public static void MigrateUsersFromAzure(PrsDbContext prsDbContext, IGraphService graph)
    {
        // Get the users from the database 
        List<string> users = [.. prsDbContext.AppUsers.Select(x => x.Email)];
        users = [.. users.Where(x => x == "jordan@ws1.com.au")];

        Dictionary<string, string> azureUsers = graph.GetUsers().Result;
        ;
    }

}
