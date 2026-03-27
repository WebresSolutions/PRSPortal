using Portal.Shared.DataEnums;
using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.DTO.Schedule;

public class UpdateScheduleTrackDto
{
    /// <summary>
    /// Used to determine if the schedule track is going to be used for creating or updating
    /// </summary>
    public int ScheduleTrackId { get; set; }

    [Required]
    public JobTypeEnum JobTypeEnum { get; set; }

    /// <summary>
    /// The date of the schedule track
    /// </summary>
    [Required]
    public DateOnly Date { get; set; }

    /// <summary>
    /// The users assigned to the schedule track
    /// </summary>
    public List<int> AssignedUsers { get; set; } = [];
}
