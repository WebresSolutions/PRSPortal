using Microsoft.EntityFrameworkCore;
using Migration.Display;
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
    public FrozenDictionary<int, int> MigrateUsers(Action<MigrationProgress>? progressCallback = null)
    {
        progressCallback?.Invoke(new MigrationProgress
        {
            CurrentStep = "Migrating Users",
            CurrentItem = "Loading users from source database...",
            CurrentItemIndex = 0,
            TotalItems = 0
        });

        // Get the users from the source database
        User[] sourceUsers = [.. _sourceDBContext.Users.AsNoTracking()];

        progressCallback?.Invoke(new MigrationProgress
        {
            CurrentStep = "Migrating Users",
            CurrentItem = $"Found {sourceUsers.Length} users",
            CurrentItemIndex = 0,
            TotalItems = sourceUsers.Length
        });

        // Get the users from the destination database
        if (sourceUsers.Length == _destinationContext.AppUsers.Count())
        {
            progressCallback?.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Users",
                CurrentItem = "Users already migrated",
                CurrentItemIndex = sourceUsers.Length,
                TotalItems = sourceUsers.Length
            });
            return _destinationContext.AppUsers.ToFrozenDictionary(x => x.LegacyUserId, y => y.LegacyUserId);
        }

        progressCallback?.Invoke(new MigrationProgress
        {
            CurrentStep = "Migrating Users",
            CurrentItem = "Creating user records...",
            CurrentItemIndex = 0,
            TotalItems = sourceUsers.Length
        });

        DateTime now = DateTime.UtcNow;
        List<AppUser> destinationUsers = [];

        for (int i = 0; i < sourceUsers.Length; i++)
        {
            User sourceUser = sourceUsers[i];
            AppUser newUser = new()
            {
                IdentityId = Guid.NewGuid().ToString(),
                Email = Guid.NewGuid().ToString(),
                DisplayName = $"{sourceUser.Firstname} {sourceUser.Lastname}",
                CreatedAt = now,
                LegacyUserId = (int)sourceUser.Id
            };
            destinationUsers.Add(newUser);

            progressCallback?.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Users",
                CurrentItem = $"Processing user: {newUser.DisplayName}",
                CurrentItemIndex = i + 1,
                TotalItems = sourceUsers.Length
            });
        }

        progressCallback?.Invoke(new MigrationProgress
        {
            CurrentStep = "Migrating Users",
            CurrentItem = "Saving users to destination database...",
            CurrentItemIndex = sourceUsers.Length,
            TotalItems = sourceUsers.Length
        });

        // Add the users and save them
        _destinationContext.AppUsers.AddRange(destinationUsers);
        _ = _destinationContext.SaveChanges();

        progressCallback?.Invoke(new MigrationProgress
        {
            CurrentStep = "Migrating Users",
            CurrentItem = $"Successfully migrated {destinationUsers.Count} users",
            CurrentItemIndex = destinationUsers.Count,
            TotalItems = destinationUsers.Count
        });

        return destinationUsers.ToFrozenDictionary(x => x.LegacyUserId, y => y.Id);
    }
}
