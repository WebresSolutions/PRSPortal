using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Types;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Instances;

public class UserService(PrsDbContext _dbContext, IXeroIntegrationService _xeroIntegrationService, ILogger<UserService> _logger) : IUserService
{
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<Result<UserJobsListDto>> GetUserJobs(int userId, HttpContext httpContext)
    {
        Result<UserJobsListDto> result = new();
        try
        {
            if (userId is 0)
                userId = httpContext.UserId();
            UserJobsListDto val = new()
            {
                UserId = userId,
                UserJobs = await _dbContext.JobUsers
                .Where(x => x.UserId == userId)
                .Select(x => new UserJobDto(x.UserId, x.JobId, new ListJobDto()
                {
                    JobId = x.JobId,
                    Address = new AddressDto(x.Job.AddressId ?? 1, (StateEnum)x.Job.Address!.StateId!, x.Job.Address.StateId ?? 3, x.Job.Address.Suburb, x.Job.Address.Street, x.Job.Address.PostCode),
                    JobNumber = x.Job.JobNumber,
                    JobType = x.Job.JobTypes.Select(x => (JobTypeEnum)x.Id).ToArray(),
                    JobTypeStatus = x.Job.Status != null ? new JobTypeStatusDto(x.Job.Status.Id, x.Job.Status.JobTypeId, x.Job.Status.Name, x.Job.Status.Sequence, x.Job.Status.Colour, x.Job.Status.IsActive) : null,
                }))
                .ToArrayAsync()
            };

            return result.SetValue(val);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user assigned jobs");
            return result.SetError(ErrorType.InternalError, "Failed to get user assigned jobs");
        }
    }
}
