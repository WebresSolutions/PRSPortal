using System.Reflection.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Portal.Server.Data;
using Portal.Server.Models;

namespace Portal.Server.Helpers;

public static class Seeder
{

    private static readonly string _staffString = "staff";
    private static readonly string _admin = "admin";

    public static void Seed(this WebApplication app)
    {
        // Seed database with an initial client application and test user
        using IServiceScope scope = app.Services.CreateScope();
        AuthDBContext? authDB = scope.ServiceProvider.GetRequiredService<AuthDBContext>();
        RoleManager<IdentityRole>? roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        UserManager<ApplicationUser>? userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Call the database seeder (made this awaitable)
        SeedDatabase(roleManager, userManager).Wait();
    }

    /// <summary>
    /// Used to seed the database with temporary users and roles
    /// </summary>
    /// <param name="roleManager">The rolemanager used for managing roles</param>
    /// <param name="userManager">User manager used for creating and updating users</param>
    /// <param name="casimoDB">The casimo database</param>
    /// <returns></returns>
    public static async Task SeedDatabase(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager)
    {
        try
        {
            // Create roles
            IdentityRole? adminRole = await roleManager.FindByNameAsync(_admin);
            if (adminRole is null)
                _ = await roleManager.CreateAsync(new IdentityRole(_admin));

            IdentityRole? staffRole = await roleManager.FindByNameAsync(_staffString);
            if (staffRole is null)
                _ = await roleManager.CreateAsync(new IdentityRole(_staffString));

            // Create some users for the roles
            string adminUsername = "admin@prs.com";
            string staffUsername = "staff@prs.com";

            ApplicationUser? adminUser = await userManager.FindByNameAsync(adminUsername);
            ApplicationUser? staffUser = await userManager.FindByNameAsync(staffUsername);

            if (adminUser is null)
            {
                ApplicationUser newUser = new()
                {
                    UserName = adminUsername,
                    Email = adminUsername,
                    EmailConfirmed = true,
                    PhoneNumber = "121121212"
                };
                IdentityResult res = await userManager.CreateAsync(newUser, "Abc.123");
                adminUser = newUser;
                // add the roles
                if (!await userManager.IsInRoleAsync(adminUser, _admin))
                {
                    IdentityResult addToRoleResult = await userManager.AddToRoleAsync(adminUser, _admin);
                }
            }

            if (staffUser is null)
            {
                ApplicationUser newUser = new()
                {
                    UserName = staffUsername,
                    Email = staffUsername,
                    EmailConfirmed = true,
                    PhoneNumber = "121121212"
                };
                IdentityResult res = await userManager.CreateAsync(newUser, "Abc.123");
                staffUser = newUser;
                // add the roles
                if (!await userManager.IsInRoleAsync(staffUser, _staffString))
                {
                    IdentityResult addToRoleResult = await userManager.AddToRoleAsync(staffUser, _staffString);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.Message);
            throw;
        }
    }
}
