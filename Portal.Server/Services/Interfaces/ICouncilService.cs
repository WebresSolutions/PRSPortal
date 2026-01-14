using Portal.Shared;
using Portal.Shared.DTO.Councils;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

public interface ICouncilService
{
    Task<Result<CouncilDetailsDto>> GetCouncilDetails(int councilId);
    Task<Result<CouncilPartialDto[]>> GetCouncils();
    Task<Result<PagedResponse<ListJobDto>>> GetCouncilJobs(int councilId, int page, int pageSize, SortDirectionEnum? order, string? orderby);
}