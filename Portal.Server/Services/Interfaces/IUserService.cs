using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

public interface IUserService
{
    Task<Result<UserDto[]>> GetUsers(bool activeOnly = true);

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
    Task<Result<List<JobNoteDto>>> GetUserAssignedJobsNotes(HttpContext httpContext, int userId, bool deleted = false, bool? actionRequired = null);

    /// <summary>
    /// Gets the user jobs for a user
    /// </summary>
    /// <param name="userId">The user id. If set to 0 will use the Id of the user calling the endpoind</param>
    /// <param name="httpContext">The http context of the user calling the endpoint</param>
    /// <returns>A result containing the collection of jobs assigned for the user</returns>
    Task<Result<UserJobsListDto>> GetUserJobs(int userId, HttpContext httpContext);
}
