using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Portal.Server.Models;

namespace Portal.Server.Data;

/// <summary>
/// Database context for authentication and identity management
/// Extends IdentityDbContext to provide user authentication and authorization data access
/// </summary>
public class AuthDBContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// Initializes a new instance of the AuthDBContext class
    /// </summary>
    /// <param name="options">The database context options for configuration</param>
    public AuthDBContext(DbContextOptions<AuthDBContext> options)
        : base(options)
    {
    }
}
