using Microsoft.EntityFrameworkCore;
using Migration.SourceDb;
using Portal.Data;
using Portal.Data.Models;
using System.Collections.Frozen;

namespace Migration.MigrationServices;

internal class Users(PrsDbContext destinationContext, SourceDBContext sourceDBContext) : BaseMigrationClass(destinationContext, sourceDBContext)
{
    /// <summary>
    /// This will be a small set so returning an in memory dictionary is fine.
    /// </summary>
    /// <returns></returns>
    public FrozenDictionary<int, int> MigrateUsers()
    {
        // Get the users from the source database
        User[] sourceUsers = [.. _sourceDBContext.Users.AsNoTracking().Where(x => x.Active == true)];

        // Get the users from the destination database
        if (sourceUsers.Length == _destinationContext.AppUsers.Count())
            return _destinationContext.AppUsers.ToFrozenDictionary(x => x.LegacyUserId, y => y.LegacyUserId);

        Console.WriteLine($"Users Found: {sourceUsers.Length}");
        DateTime now = DateTime.UtcNow;
        AppUser[] destinationUsers = [.. sourceUsers.Select(x => new AppUser
            {
                IdentityId = Guid.NewGuid().ToString(),
                Email = Guid.NewGuid().ToString(),
                DisplayName = $"{x.Firstname} {x.Lastname}",
                CreatedAt = x.Created,
                LegacyUserId = (int)x.Id
        })];
        // Add the users and save them
        _destinationContext.AppUsers.AddRange(destinationUsers);
        _ = _destinationContext.SaveChanges();

        return destinationUsers.ToFrozenDictionary(x => x.LegacyUserId, y => y.LegacyUserId);
    }
}
