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
}
