using Portal.Shared.DTO.Councils;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

public interface ICouncilService
{
    Task<Result<CouncilDetailsDto>> GetCouncilDetails(int councilId);
    Task<Result<CouncilPartialDto[]>> GetCouncils();
}