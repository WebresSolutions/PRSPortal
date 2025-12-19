using Heron.MudCalendar;
using Portal.Shared.DTO.Schedule;

namespace Portal.Client.Webmodels;

public class ScheduleSlotDtoWithCalendar : ScheduleSlotDTO
{
    public List<CustomCalendarItem> Events { get; set; } = [];

    public void SetEvents()
    {
        Events = [.. base.Schedule.Select(s => new CustomCalendarItem
        {
            Start = s.Start,
            End = s.End,
            Text = s.Description,
            Colour = s.Colour.ColourHex,
            JobNumber = s.Job?.JobNumber,
            JobAddress = s.Job?.Address is not null ? $"{s.Job.Address.street}, {s.Job.Address.suburb}, {s.Job.Address.postCode}" : string.Empty
        })];
    }
}


public class CustomCalendarItem : CalendarItem
{
    public required string Colour { get; set; }
    public int? JobNumber { get; set; }
    public string JobAddress { get; set; } = string.Empty;
}