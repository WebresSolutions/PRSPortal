using Microsoft.AspNetCore.Identity;

namespace Portal.Server.Database.Identity;

/// <summary>
/// Extended user model for the Casimo application
/// Inherits from IdentityUser to provide custom user properties for temporary user management
/// </summary>
public class ApplicationUser : IdentityUser
{
}
