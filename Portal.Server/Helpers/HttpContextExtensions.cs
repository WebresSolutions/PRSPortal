using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Data.Models;

namespace Portal.Server.Helpers;

public static class HttpContextExtensions
{
    public static string GetMyUserId(this HttpContext httpContext)
    {
        string res = httpContext.User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value ?? string.Empty;
        return res;
    }

    public static async Task<int> GetMyUserIdAsInt(this HttpContext httpContext, PrsDbContext context)
    {
        string userIdStr = httpContext.GetMyUserId();
        AppUser? appUser = await context.AppUsers.FirstOrDefaultAsync(u => u.IdentityId == userIdStr);
        return appUser?.Id ?? throw new Exception("Could not find the logged in users ID");
    }
}
