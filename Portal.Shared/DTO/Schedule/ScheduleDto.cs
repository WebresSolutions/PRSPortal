namespace Portal.Shared.DTO.Schedule;

public class ScheduleDto
{
    public int ScheduleId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int? ScheduleSlotID { get; set; }
    public required ScheduleColourDto Colour { get; set; }
    public string Description { get; set; } = string.Empty;
    public ScheduleJobPartialDto? Job { get; set; }
}
