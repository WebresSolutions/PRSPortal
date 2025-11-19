using Portal.Shared;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Services.Interfaces;

public interface IApiService
{
    Task<Result<PagedResponse<ListJobDto>>> GetAllJobs(int pageSize, int pageNumber, string? nameFilter, string? orderby, SortDirectionEnum order);
}
