using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.File;
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
    Task<Result<PagedResponse<ListJobDto>>> GetAllJobs(JobFilterDto filter);

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
    /// Updates a job.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="updateJobDto"></param>
    /// <returns></returns>
    Task<Result<JobDetailsDto>> UpdateJob(HttpContext httpContext, JobUpdateDto updateJobDto);

    /// <summary>
    /// Create a new job with the specified details and return the unique identifier of the created job.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="jobCreationDto">Job creation payload.</param>
    /// <param name="manageTransaction">When true (default), begins and commits a database transaction. When false, uses the caller's ambient EF Core transaction (no nested transaction).</param>
    /// <returns>The new job id or an error.</returns>
    Task<Result<int>> CreateJob(HttpContext httpContext, JobCreationDto jobCreationDto, bool manageTransaction = true);

    /// <summary>
    /// Will soft delete a job
    /// </summary>
    /// <param name="httpContext">The context of the user performing the deletion</param>
    /// <param name="id">The Id of the job to delete</param>
    /// <returns></returns>
    Task<Result<bool>> DeleteJob(HttpContext httpContext, int id);

    /// <summary>
    /// Gets technical Contacts for a job, if contactId is provided it will get the specific technical contact, 
    /// otherwise it will get all technical contacts for the job. If jobId is not provided it will get all technical contacts for the contactId provided.
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="contactId"></param>
    /// <returns></returns>
    Task<Result<TechnicalContactDto[]>> GetTechnicalContacts(int? jobId, int? contactId, bool showDeleted = false);

    /// <summary>
    /// Creates a new technical contact using the specified data and returns the updated list of technical contacts.
    /// </summary>
    /// <param name="dto">An object containing the information required to create the new technical contact. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}">Result</see>
    /// object with an array of <see cref="TechnicalContactDto"/> representing all technical contacts after the
    /// addition. The array is empty if no contacts are available.</returns>
    Task<Result<TechnicalContactDto[]>> NewTechnicalContact(HttpContext httpContext, SaveTechnicalContactTypeDto dto);

    /// <summary>
    /// Updates the technical contact information using the specified data transfer object.
    /// </summary>
    /// <param name="dto">An object containing the updated technical contact information to be saved. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array of updated technical
    /// contact data transfer objects. The array will be empty if no contacts were updated.</returns>
    Task<Result<TechnicalContactDto[]>> UpdateTechnicalContact(HttpContext httpContext, SaveTechnicalContactTypeDto dto);

    /// <summary>
    /// Uploads file for a job
    /// </summary>
    /// <param name="jobId">The Job Id this is being uploaded to</param>
    /// <param name="file">The file being uploaded</param>
    /// <param name="context">The http context for validateing who is saving</param>
    /// <returns>The job file ID</returns>
    Task<Result<int>> SaveJobFile(HttpContext context, int jobId, FileDto file);
}
