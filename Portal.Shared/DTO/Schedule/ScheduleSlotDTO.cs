using Portal.Shared.DTO.User;

namespace Portal.Shared.DTO.Schedule;

public class ScheduleSlotDTO
{
    public int SlotId { get; set; }
    public DateOnly Day { get; set; }
    public List<ScheduleDto> Schedule { get; set; } = [];
    public List<UserDto> AssignedUsers { get; set; } = [];
}
