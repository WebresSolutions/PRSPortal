using Portal.Shared.DTO.Address;

namespace Portal.Shared.DTO.Schedule;

public class ScheduleJobPartialDto
{
    public int JobId { get; set; }
    public AddressDTO? Address { get; set; }
    public int? JobNumber { get; set; }
}
