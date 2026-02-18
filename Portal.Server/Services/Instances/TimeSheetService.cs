using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.ResponseModels;
using System.Linq.Dynamic.Core;

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
            if (endDate < startDate)
                return res.SetError(ErrorType.BadRequest, "End date cannot be before start date");

            if (userId is 0)
                userId = httpContext.UserId();

            // Ensure they are UTC and the 'Kind' is explicitly set to Utc
            startDate = DateTime.SpecifyKind(startDate.ToUniversalTime(), DateTimeKind.Utc);

            if (endDate.HasValue)
                endDate = DateTime.SpecifyKind(endDate.Value.ToUniversalTime(), DateTimeKind.Utc);
            else
                endDate = DateTime.SpecifyKind(startDate.AddDays(7), DateTimeKind.Utc);

            // Get all timesheets from the user
            res.Value = await _dbContext.TimesheetEntries
                .Where(x => x.UserId == userId
                    && x.DateFrom >= startDate
                    && (x.DateTo <= endDate || x.DateTo == null))
                .Select(t => new TimeSheetDto(t.Id, t.DateFrom, t.DateTo, userId, t.JobId, t.Description))
                .ToArrayAsync();

            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get timesheets ex: {}", ex);
            return res.SetError(ErrorType.InternalError, "An internal error occured while getting timesheets");
        }
    }

    public async Task<Result<TimeSheetDto>> AddTimeSheetEntry(HttpContext httpContext, TimeSheetEntryDto entry)
    {
        Result<TimeSheetDto> res = new();
        try
        {
            int userId = (entry.userId is 0 or null) ? httpContext.UserId() : entry.userId.Value;

            DateTime utcStart = DateTime.SpecifyKind(entry.start.ToUniversalTime(), DateTimeKind.Utc);

            DateTime? utcEnd = entry.end.HasValue
                ? DateTime.SpecifyKind(entry.end.Value.ToUniversalTime(), DateTimeKind.Utc)
                : null;

            // Validation: Check for existing active entries
            if (utcEnd == null && await _dbContext.TimesheetEntries.AnyAsync(t => t.UserId == userId && t.DateTo == null))
                return res.SetError(ErrorType.BadRequest, "There is already an active timesheet entry for this user.");

            if (entry.jobId is not null && !await _dbContext.Jobs.AnyAsync(x => x.Id == entry.jobId))
                return res.SetError(ErrorType.BadRequest, "The provided jobId does not exist");

            TimesheetEntry newEntry = new()
            {
                UserId = userId,
                DateFrom = utcStart,
                DateTo = utcEnd,
                Description = entry.description,
                JobId = entry.jobId,
                CreatedByUserId = httpContext.UserId(),
                ModifiedByUserId = httpContext.UserId(),
            };

            await _dbContext.TimesheetEntries.AddAsync(newEntry);
            await _dbContext.SaveChangesAsync();

            res.Value = new TimeSheetDto(newEntry.Id, newEntry.DateFrom, newEntry.DateTo, userId, newEntry.JobId, newEntry.Description);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create timesheet ex: {Ex}", ex);
            return res.SetError(ErrorType.InternalError, "An error occurred while creating a timesheet");
        }
    }

    public async Task<Result<bool>> RemoveTimeSheetEntry(HttpContext httpContext, int id)
    {
        Result<bool> res = new();
        try
        {

            await _dbContext.TimesheetEntries
                .Where(t => t.Id == id)
                .ExecuteDeleteAsync();

            res.Value = true;
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create timesheet ex: {Ex}", ex);
            return res.SetError(ErrorType.InternalError, "An error occurred while creating a timesheet");
        }
    }


    public Task<Result<TimeSheetDto[]>> GetAllTimeSheets(HttpContext httpContext, DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }
}
