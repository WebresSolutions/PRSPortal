using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Instances;

public class UserService(PrsDbContext _dbContext, ILogger<UserService> _logger) : IUserService
{
    /// <summary>
    /// Gets all users
    /// </summary>
    /// <param name="activeOnly">Flag if only getting the active users</param>
    /// <returns>A collection of User DTOs</returns>
    public async Task<Result<UserDto[]>> GetUsers(bool activeOnly = true)
    {
        Result<UserDto[]> res = new();
        try
        {
            res.Value = await _dbContext.AppUsers
                .Where(x => (activeOnly && x.DeactivatedAt == null) || !activeOnly)
                .Select(u => new UserDto(u.Id, u.DisplayName, u.DeactivatedAt == null))
                .ToArrayAsync();

            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get users ex: {}", ex);
            return res.SetError(ErrorType.InternalError, "Internal server error occurred while getting the users");
        }
    }
}
