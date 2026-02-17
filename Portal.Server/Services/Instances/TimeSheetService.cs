using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Instances;

public class TimeSheetService(PrsDbContext _dbContext, ILogger<TimeSheetService> _logger) : ITimeSheetService
{
    /// <summary>
    /// Gets user time sheets
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<Result<TimeSheetDto[]>> GetUserTimeSheets(HttpContext httpContext, DateTime startDate, DateTime? endDate, int userId = 0)
    {
        Result<TimeSheetDto[]> res = new();
        try
        {
            if (userId is 0)
                userId = httpContext.UserId();

            endDate ??= startDate.AddDays(7);

            // Get all timesheets from the user
            res.Value = await _dbContext.TimesheetEntries
                .Where(x => x.UserId == userId && startDate >= x.DateFrom && (endDate <= x.DateTo || x.DateTo == null))
                .Select(t => new TimeSheetDto(t.DateFrom, t.DateTo, userId, t.JobId, t.Description))
                .ToArrayAsync();

            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get timesheets ex: {}", ex);
            return res.SetError(ErrorType.InternalError, "An internal error occured while getting timesheets");
        }
    }

    public async Task<Result<TimeSheetDto>> CreateTimeSheet(HttpContext httpContext, int userId, TimeSheetDto data)
    {
        Result<TimeSheetDto> res = new();
        try
        {
            if (userId is 0)
                userId = httpContext.UserId();


            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create timesheets ex: {}", ex);
            return res.SetError(ErrorType.InternalError, "An error occurred while creating a timesheet");
        }
    }

    public Task<Result<TimeSheetDto[]>> GetAllTimeSheets(HttpContext httpContext, DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }
}
