using Portal.Shared.DTO.User;

namespace Portal.Shared.DTO.Schedule;

/// <summary>
/// Data transfer object representing a schedule slot
/// Contains a time slot with associated schedules and assigned users
/// </summary>
public class ScheduleSlotDTO
{
    /// <summary>
    /// Gets or sets the unique identifier for the schedule slot
    /// </summary>
    public int SlotId { get; set; }
    /// <summary>
    /// Gets or sets the date of the schedule slot
    /// </summary>
    public DateOnly Day { get; set; }
    /// <summary>
    /// Gets or sets the list of schedules within this slot
    /// </summary>
    public List<ScheduleDto> Schedule { get; set; } = [];
    /// <summary>
    /// Gets or sets the list of users assigned to this schedule slot
    /// </summary>
    public List<UserDto> AssignedUsers { get; set; } = [];
}
