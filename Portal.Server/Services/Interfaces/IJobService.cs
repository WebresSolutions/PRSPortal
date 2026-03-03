using Portal.Shared;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

public interface IJobService
{
    /// <summary>
    /// Get all jobs with pagination, sorting, and filtering options.
    /// </summary>
    /// <param name="page">The page number</param>
    /// <param name="pageSize">The page size</param>
    /// <param name="order">Order by </param>
    /// <param name="nameFilter">The name filter</param>
    /// <param name="orderby">Column to order by</param>
    /// <returns>A paged list of responses</returns>
    Task<Result<PagedResponse<ListJobDto>>> GetAllJobs(
        int page,
        int pageSize,
        SortDirectionEnum? order,
        string? addressSearch,
        string? contactSearch,
        string? jobNumberSearch,
        string? orderby,
        bool deleted = false);

    /// <summary>
    /// Retrieves detailed information for a job specified by its unique identifier.
    /// </summary>
    /// <remarks>The returned job details include associated notes and address information. If the specified
    /// job does not exist or has been deleted, the result will indicate a 'NotFound' error.</remarks>
    /// <param name="jobId">The unique identifier of the job to retrieve. Must refer to an existing, non-deleted job.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{JobDetailsDto}"/> with job details if found; otherwise, an error indicating that the job was not
    /// found.</returns>
    Task<Result<JobDetailsDto>> GetJob(int jobId);

    /// <summary>
    /// Gets the notes for a job by job id.
    /// </summary>
    /// <param name="jobId">The job id.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted notes.</param>
    /// <returns>The list of notes for the job.</returns>
    Task<Result<List<JobNoteDto>>> GetJobNotes(int jobId, bool includeDeleted = false, bool? actionRequired = null);

    /// <summary>
    /// Creates a new note
    /// </summary>
    /// <param name="httpContext">The httpcontext</param>
    /// <param name="note">The note to update</param>
    /// <returns></returns>
    Task<Result<List<JobNoteDto>>> CreateNote(HttpContext httpContext, JobNoteDto note);

    /// <summary>
    /// Updates a note, this can be used to update the content, action required, assigned user or deleted status of a note
    /// </summary>
    /// <param name="httpContext">The httpcontext</param>
    /// <param name="note">The note to update</param>
    /// <returns>The job details</returns>
    Task<Result<List<JobNoteDto>>> UpdateNote(HttpContext httpContext, JobNoteDto note);

    /// <summary>
    /// Gets the notes for jobs assigned to a specific user.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="userId"></param>
    /// <param name="includeDeleted"></param>
    /// <returns></returns>
    Task<Result<List<JobNoteDto>>> GetUserAssignedJobsNotes(HttpContext httpContext, int userId, bool includeDeleted);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="updateJobDto"></param>
    /// <returns></returns>
    Task<Result<JobDetailsDto>> UpdateJob(HttpContext httpContext, JobDetailsDto updateJobDto);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="jobCreationDto"></param>
    /// <returns></returns>
    Task<Result<int>> CreateJob(HttpContext httpContext, JobCreationDto jobCreationDto);

    /// <summary>
    /// Will soft delete a job
    /// </summary>
    /// <param name="httpContext">The context of the user performing the deletion</param>
    /// <param name="id">The Id of the job to delete</param>
    /// <returns></returns>
    Task<Result<bool>> DeleteJob(HttpContext httpContext, int id);
}
