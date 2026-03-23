using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Instances;

public class UserService(PrsDbContext _dbContext, IXeroIntegrationService _xeroIntegrationService, ILogger<UserService> _logger) : IUserService
{
    /// <summary>
    /// Gets all users
    /// </summary>
    /// <param name="activeOnly">Flag if only getting the active users</param>
    /// <returns>A collection of User DTOs</returns>
    public async Task<Result<UserDto[]>> GetUsersWithLeave(bool activeOnly = true)
    {
        Result<UserDto[]> res = new();
        try
        {
            res.Value = await _dbContext.AppUsers
                .Where(x => (activeOnly && x.DeactivatedAt == null) || !activeOnly)
                .OrderBy(x => x.DisplayName)
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

    public async Task<Result<UserDto[]>> GetUsers(bool activeOnly = true)
    {
        Result<UserDto[]> res = new();
        try
        {
            res.Value = await _dbContext.AppUsers
                .Where(x => (activeOnly && x.DeactivatedAt == null) || !activeOnly)
                .OrderBy(x => x.DisplayName)
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

    /// <summary>
    /// Retrieves the list of job notes assigned to a specified user.
    /// </summary>
    /// <param name="httpContext">The HTTP context containing user information. Used to determine the user if <paramref name="userId"/> is 0.</param>
    /// <param name="userId">The identifier of the user whose assigned job notes are to be retrieved. If 0, the user ID is obtained from the
    /// HTTP context.</param>
    /// <param name="includeDeleted">A value indicating whether to include deleted job notes in the result. If <see langword="true"/>, deleted notes
    /// are included; otherwise, only active notes are returned.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/> with a
    /// list of <see cref="JobNoteDto"/> objects assigned to the specified user. If no notes are found, the list is
    /// empty.</returns>
    public async Task<Result<List<JobNoteDto>>> GetUserAssignedJobsNotes(HttpContext httpContext, int userId, bool deleted = false, bool? actionRequired = null)
    {
        Result<List<JobNoteDto>> result = new();
        try
        {
            if (userId is 0)
                userId = httpContext.UserId();

            IQueryable<JobNote> query = _dbContext.JobNotes
                .AsNoTracking()
                .Where(n => n.AssignedUserId == userId);

            query = deleted ? query.Where(n => n.DeletedAt != null) : query.Where(n => n.DeletedAt == null);

            if (actionRequired.HasValue)
                query = query.Where(n => n.ActionRequired == actionRequired.Value);

            result.Value = await query
                .Select(n => new JobNoteDto
                {
                    NoteId = n.Id,
                    JobId = n.JobId,
                    Content = n.Note,
                    AssignedUser = n.AssignedUserId != null
                        ? new(n.AssignedUserId.Value, n.AssignedUser!.DisplayName ?? "")
                        : null,
                    DateCreated = n.CreatedOn,
                    ActionRequired = n.ActionRequired,
                    Deleted = n.DeletedAt != null,
                    DateModified = n.ModifiedOn,
                    CreatedBy = n.CreatedByUser.DisplayName,
                    Modifiedby = n.ModifiedByUser != null ? n.ModifiedByUser.DisplayName : null
                })
                .OrderByDescending(x => x.DateCreated)
                .ToListAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user assigned job notes");
            return result.SetError(ErrorType.InternalError, "Failed to get user assigned job notes");
        }
    }
}
