using Portal.Shared;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

/// <summary>
/// Interface for schedule-related business operations
/// Defines methods for managing schedules and schedule colors
/// </summary>
public interface IScheduleService
{
    /// <summary>
    /// Retrieves schedule slots for a specific date and job type
    /// </summary>
    /// <param name="date">The date to retrieve schedule slots for</param>
    /// <param name="jobType">The job type to filter by</param>
    /// <returns>A result containing a list of schedule slot DTOs</returns>
    Task<Result<List<ScheduleTrackDto>>> GetScheduleSlotsForDate(HttpContext context, DateOnly date, JobTypeEnum jobType);
    /// <summary>
    /// Retrieves all available schedule colors
    /// </summary>
    /// <returns>A result containing a list of schedule color DTOs</returns>
    Task<Result<List<ScheduleColourDto>>> GetScheduleColours();
    /// <summary>
    /// Updates an existing schedule color or creates a new one
    /// </summary>
    /// <param name="colour">The schedule color DTO to update or create</param>
    /// <returns>A result containing the updated or created schedule color DTO</returns>
    Task<Result<ScheduleColourDto>> UpdateScheduleColour(ScheduleColourDto colour);
    /// <summary>
    /// Updates a schedule on a schedule track
    /// </summary>
    /// <param name="context">The http context of the calling user</param>
    /// <param name="data"></param>
    /// <returns></returns>
    Task<Result<int>> UpdateSchedule(HttpContext context, UpdateScheduleDto data);
    /// <summary>
    /// Updates a schedule slote
    /// </summary>
    /// <param name="context">The http context of the calling user</param>
    /// <param name="data">Updates a schedule slot </param>
    /// <returns>The updates schedule track</returns>
    Task<Result<ScheduleTrackDto>> UpdateScheduleTrack(HttpContext context, UpdateScheduleTrackDto data);
    /// <summary>
    /// Gets the schedule for the week. 
    /// </summary>
    /// <param name="jobType">The job type</param>
    /// <param name="weekDay">The weekday to get the schedule for</param>
    /// <returns>An array of schedules for the week</returns>
    Task<Result<WeeklyScheduleDto[]>> GetWeeklySchedule(JobTypeEnum jobType, DateOnly? weekDay);
}
