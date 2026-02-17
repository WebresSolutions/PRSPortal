using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

public interface ITimeSheetService
{
    Task<Result<TimeSheetDto[]>> GetUserTimeSheets(HttpContext httpContext, DateTime startDate, DateTime? endDate, int userId = 0);

    Task<Result<TimeSheetDto[]>> GetAllTimeSheets(HttpContext httpContext, DateTime startDate, DateTime endDate);
}
