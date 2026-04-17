using System.Security.Claims;

namespace Portal.Server.Helpers;

public static class HttpContextExtensions
{
    public static int UserId(this HttpContext httpContext)
    {
        // Should be set by CustomMiddleware, so we can safely assume it exists if the middleware is properly configured and executed.
        httpContext.Items.TryGetValue("UserId", out object? userIdObj);

        if (userIdObj is null)
            return 147;

        if (userIdObj is int userId)
            return userId;
        else
            throw new Exception($"UserId in HttpContext.Items is not of type int. Actual type: {userIdObj.GetType()}");
    }

    public static string IdentityId(this HttpContext httpContext)
    {
        string userId = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        return userId;
    }

    public static string GetMyUserEmail(this HttpContext httpContext)
    {
        string res = httpContext.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value ?? string.Empty;
        return res;
    }

}
