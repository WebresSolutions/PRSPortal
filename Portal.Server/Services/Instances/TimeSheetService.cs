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
                .Select(t => new TimeSheetDto(t.Id, t.TypeId, t.DateFrom, t.DateTo, userId, t.JobId, t.Description, "", t.Job != null ? t.Job.JobNumber : 0))
                .ToArrayAsync();

            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get timesheets ex: {}", ex);
            return res.SetError(ErrorType.InternalError, "An internal error occured while getting timesheets");
        }
    }

    /// <summary>
    /// Adds a timesheet for the user
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="entry"></param>
    /// <returns></returns>
    public async Task<Result<TimeSheetDto>> AddTimeSheetEntry(HttpContext httpContext, TimeSheetDto entry)
    {
        Result<TimeSheetDto> res = new();
        try
        {
            int userId = (entry.UserId is 0) ? httpContext.UserId() : entry.UserId;

            DateTime utcStart = DateTime.SpecifyKind(entry.Start.ToUniversalTime(), DateTimeKind.Utc);

            DateTime? utcEnd = entry.End.HasValue
                ? DateTime.SpecifyKind(entry.End.Value.ToUniversalTime(), DateTimeKind.Utc)
                : null;

            // Validate the type Id
            if (!await _dbContext.TimesheetEntryTypes.AnyAsync(x => x.Id == entry.TypeId))
                return res.SetError(ErrorType.BadRequest, "Invalid Entry Type");

            // Validation: Check for existing active entries
            if (utcEnd == null && await _dbContext.TimesheetEntries.AnyAsync(t => t.UserId == userId && t.DateTo == null))
                return res.SetError(ErrorType.BadRequest, "There is already an active timesheet entry for this user.");

            if (entry.JobId is not null && !await _dbContext.Jobs.AnyAsync(x => x.Id == entry.JobId))
                return res.SetError(ErrorType.BadRequest, "The provided jobId does not exist");

            TimesheetEntry newEntry = new()
            {
                UserId = userId,
                TypeId = entry.TypeId,
                DateFrom = utcStart,
                DateTo = utcEnd,
                Description = StringNormalizer.TrimAndTruncate(entry.Description, 4000),
                JobId = entry.JobId,
                CreatedByUserId = httpContext.UserId(),
                ModifiedByUserId = httpContext.UserId(),
            };

            await _dbContext.TimesheetEntries.AddAsync(newEntry);
            await _dbContext.SaveChangesAsync();

            res.Value = new TimeSheetDto(newEntry.Id, entry.TypeId, newEntry.DateFrom, newEntry.DateTo, userId, newEntry.JobId, newEntry.Description, "", 0);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create timesheet ex: {Ex}", ex);
            return res.SetError(ErrorType.InternalError, "An error occurred while creating a timesheet");
        }
    }

    /// <summary>
    /// Updates a timesheet entry. Only the user that created the entry or an application administrator can update it.
    /// </summary>
    /// <param name="httpContext">The http context</param>
    /// <param name="entry">The TimeSheetentry being updated</param>
    /// <returns>A timesheet dto</returns>
    public async Task<Result<TimeSheetDto>> UpdateTimeSheet(HttpContext httpContext, TimeSheetDto entry)
    {
        Result<TimeSheetDto> res = new();
        try
        {
            if (entry is { Id: 0 })
                return res.SetError(ErrorType.BadRequest, "Timesheet entry id is required for update");

            if (entry.End is not null && entry.End < entry.Start)
                return res.SetError(ErrorType.BadRequest, "End date cannot be before start date");

            int userId = (entry.UserId is 0) ? httpContext.UserId() : entry.UserId;

            TimesheetEntry? existingEntry = await _dbContext.TimesheetEntries.FirstOrDefaultAsync(t => t.Id == entry.Id);

            if (existingEntry is null)
                return res.SetError(ErrorType.BadRequest, "Timesheet entry with the provided id does not exist");

            if (existingEntry.UserId != userId && !httpContext.User.IsInRole("Application Administrator"))
                return res.SetError(ErrorType.MissingPrivileges, "You do not have permission to update this timesheet entry");

            if (entry.JobId is not null && await _dbContext.Jobs.FirstOrDefaultAsync(x => x.Id == entry.JobId) is null)
                return res.SetError(ErrorType.BadRequest, "The provided jobId does not exist");

            // Convert the times to the UTC time
            DateTime utcStart = DateTime.SpecifyKind(entry.Start.ToUniversalTime(), DateTimeKind.Utc);
            DateTime? utcEnd = entry.End.HasValue
                ? DateTime.SpecifyKind(entry.End.Value.ToUniversalTime(), DateTimeKind.Utc)
                : null;

            existingEntry.DateFrom = utcStart;
            existingEntry.DateTo = utcEnd;
            existingEntry.Description = StringNormalizer.TrimAndTruncate(entry.Description, 4000);
            existingEntry.ModifiedByUserId = httpContext.UserId();
            existingEntry.ModifiedOn = DateTime.UtcNow;
            existingEntry.JobId = entry.JobId;

            await _dbContext.SaveChangesAsync();

            res.Value = new TimeSheetDto(existingEntry.Id, existingEntry.TypeId, existingEntry.DateFrom, existingEntry.DateTo, userId, existingEntry.JobId, existingEntry.Description, "", 0);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create timesheet ex: {Ex}", ex);
            return res.SetError(ErrorType.InternalError, "An error occurred while creating a timesheet");
        }
    }

    /// <summary>
    /// Removes an entry from the timesheet. Only the user that created the entry or an application administrator can remove it.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Result<bool>> RemoveTimeSheetEntry(HttpContext httpContext, int id)
    {
        Result<bool> res = new();
        try
        {
            TimesheetEntry? existingEntry = await _dbContext.TimesheetEntries.FirstOrDefaultAsync(t => t.Id == id);

            if (existingEntry is null)
                return res.SetError(ErrorType.BadRequest, "Timesheet entry with the provided id does not exist");

            if (existingEntry.UserId != httpContext.UserId() && !httpContext.User.IsInRole("Application Administrator"))
                return res.SetError(ErrorType.MissingPrivileges, "You do not have permission to update this timesheet entry");

            await _dbContext.TimesheetEntries
                .Where(t => t.Id == id)
                .ExecuteDeleteAsync();

            res.Value = true;
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to remove timesheet ex: {Ex}", ex);
            return res.SetError(ErrorType.InternalError, "An error occurred while updating a timesheet");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    public async Task<Result<TimeSheetDto[]>> GetAllTimeSheets(HttpContext httpContext, DateTime startDate, DateTime? endDate)
    {
        Result<TimeSheetDto[]> res = new();
        try
        {
            if (!httpContext.User.IsInRole("Application Administrator"))
                return res.SetError(ErrorType.MissingPrivileges, "You do not have permission to view all timesheet entries");

            if (endDate < startDate)
                return res.SetError(ErrorType.BadRequest, "End date cannot be before start date");

            // Ensure they are UTC and the 'Kind' is explicitly set to Utc
            startDate = DateTime.SpecifyKind(startDate.ToUniversalTime(), DateTimeKind.Utc);

            if (endDate.HasValue)
                endDate = DateTime.SpecifyKind(endDate.Value.ToUniversalTime(), DateTimeKind.Utc);
            else
                endDate = DateTime.SpecifyKind(startDate.AddDays(7), DateTimeKind.Utc);

            // Get all timesheets from the user
            res.Value = await _dbContext.TimesheetEntries
                .Where(x => x.DateFrom >= startDate && (x.DateTo <= endDate || x.DateTo == null))
                .Select(t => new TimeSheetDto(t.Id, t.TypeId, t.DateFrom, t.DateTo, t.UserId, t.JobId, t.Description, "", 0))
                .ToArrayAsync();

            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get timesheets ex: {}", ex);
            return res.SetError(ErrorType.InternalError, "An internal error occured while getting timesheets");
        }
    }



}
