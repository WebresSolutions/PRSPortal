using Microsoft.AspNetCore.Http;
using Portal.Shared.DTO.Councils;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

public interface ICouncilService
{
    Task<Result<int>> CreateCouncil(HttpContext httpContext, CouncilCreationDto data);
    Task<Result<CouncilDetailsDto>> GetCouncilDetails(int councilId);
    Task<Result<CouncilPartialDto[]>> GetCouncils();
    Task<Result<CouncilDetailsDto>> UpdateCouncil(HttpContext httpContext, CouncilUpdateDto data);
}