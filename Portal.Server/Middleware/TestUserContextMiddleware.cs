using Microsoft.Extensions.Configuration;

namespace Portal.Server.Middleware;

/// <summary>
/// Sets UserId and UserEmail on HttpContext.Items for integration tests when auth is disabled.
/// Only used when the application runs in the "Testing" environment so endpoints that rely on
/// CustomMiddleware-populated context (e.g. UserId()) work without real authentication.
/// </summary>
public class TestUserContextMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        int userId = configuration.GetValue("Testing:TestUserId", 1);
        string userEmail = configuration.GetValue<string>("Testing:TestUserEmail") ?? "testuser@example.com";

        context.Items["UserId"] = userId;
        context.Items["UserEmail"] = userEmail;

        await _next(context);
    }
}
