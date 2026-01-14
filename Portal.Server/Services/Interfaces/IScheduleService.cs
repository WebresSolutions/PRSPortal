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
    Task<Result<List<ScheduleSlotDTO>>> GetScheduleSlotsForDate(DateOnly date, JobTypeEnum jobType);
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
}
