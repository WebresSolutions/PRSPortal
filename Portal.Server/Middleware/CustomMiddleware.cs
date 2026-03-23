using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Helpers;

namespace Portal.Server.Middleware;

public class CustomMiddleware(RequestDelegate next) // Remove dbContext from constructor
{
    private readonly RequestDelegate _next = next;

    /// <summary>
    /// Middleware context to set UserId and UserEmail in HttpContext.Items for authenticated users, with caching to optimize database access.
    /// </summary>
    /// <param name="context">The http context</param>
    /// <param name="dbContext">The database context</param>
    /// <param name="memoryCache">The memory cache for caching recurring user request</param>
    /// <returns></returns>
    /// <exception cref="Exception">Throws if the user does not exist</exception>
    public async Task InvokeAsync(HttpContext context, PrsDbContext dbContext, IMemoryCache memoryCache, ILogger<Program> logger)
    {
        AuthenticateResult res = await context.AuthenticateAsync();

        if (!res.Succeeded)
        {
            await _next(context);
            return;
        }

        string identityId = context.IdentityId();
        string email = context.GetMyUserEmail();
        string cacheKey = $"User_{identityId}"; // Unique key per user

        // Try to get just this userId from cache
        if (!memoryCache.TryGetValue(cacheKey, out int userId))
        {
            // Not in cache, check DB
            AppUser? appUser = await dbContext.AppUsers.FirstOrDefaultAsync(u => u.IdentityId == identityId);

            if (appUser is null)
            {
                // IdentityId mismatch: check by email 
                appUser = await dbContext.AppUsers.FirstOrDefaultAsync(x => x.Email == email);
                if (appUser is not null)
                {
                    logger.LogWarning("Updating IdentityId for user {Email}. New ID: {IdentityId}", email, identityId);
                    appUser.IdentityId = identityId;
                    dbContext.AppUsers.Update(appUser);
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    if (email.Contains("prs.au") && !await dbContext.AppUsers.AnyAsync(x => x.IdentityId == identityId))
                    {
                        // Create a new user 
                        AppUser newUser = new()
                        {
                            IdentityId = identityId,
                            Email = email,
                            DisplayName = email.Split('@')[0],
                            CreatedOn = DateTime.UtcNow,
                        };
                        dbContext.AppUsers.Add(newUser);
                        await dbContext.SaveChangesAsync();
                        appUser = newUser;
                    }
                    else
                    {
                        logger.LogError("User with IdentityId {IdentityId} (Email: {Email}) not found in database. Throwing exception.", identityId, email);
                        throw new Exception($"Could not find user with Email {email} or IdentityId {identityId} in the database.");
                    }
                }
            }

            userId = appUser.Id;
            appUser.LastLogin = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            // Cache only this specific User's ID
            memoryCache.Set(cacheKey, userId, TimeSpan.FromMinutes(60));
        }

        context.Items["UserId"] = userId;
        context.Items["UserEmail"] = email;

        await _next(context);
    }
}