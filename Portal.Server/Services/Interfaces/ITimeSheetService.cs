using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

public interface ITimeSheetService
{
    /// <summary>
    /// Gets the time sheets for a user within a specified date range. If endDate is not provided, it will return time sheets from startDate to the current date.
    /// </summary>
    /// <param name="httpContext">The http context</param>
    /// <param name="startDate">The start date</param>
    /// <param name="endDate">The end date</param>
    /// <param name="userId">The user ID</param>
    /// <returns></returns>
    Task<Result<TimeSheetDto[]>> GetUserTimeSheets(HttpContext httpContext, DateTime startDate, DateTime? endDate, int userId = 0);

    /// <summary>
    /// Gets all time sheets for all users within a specified date range. If endDate is not provided, it will return time sheets from startDate to the current date.
    /// </summary>
    /// <param name="httpContext">The http context</param>
    /// <param name="startDate">The start date</param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    Task<Result<TimeSheetDto[]>> GetAllTimeSheets(HttpContext httpContext, DateTime startDate, DateTime? endDate);

    /// <summary>
    /// Adds a timesheet for the user
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="entry"></param>
    /// <returns></returns>
    Task<Result<TimeSheetDto>> AddTimeSheetEntry(HttpContext httpContext, TimeSheetDto entry);

    /// <summary>
    /// Updates a timesheet entry. Only the user that created the entry or an application administrator can update it.
    /// </summary>
    /// <param name="httpContext">The http context</param>
    /// <param name="entry">The TimeSheetentry being updated</param>
    /// <returns>A timesheet dto</returns>
    Task<Result<TimeSheetDto>> UpdateTimeSheet(HttpContext httpContext, TimeSheetDto entry);

    /// <summary>
    /// Removes an entry from the timesheet. Only the user that created the entry or an application administrator can remove it.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Result<bool>> RemoveTimeSheetEntry(HttpContext httpContext, int id);
}
