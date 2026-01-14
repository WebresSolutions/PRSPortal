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
    Task<Result<PagedResponse<ListJobDto>>> GetAllJobs(int page, int pageSize, SortDirectionEnum? order, string? nameFilter, string? orderby);

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
}
