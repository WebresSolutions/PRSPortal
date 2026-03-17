using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Client.Webmodels;
using Portal.Shared;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.Helpers;

namespace Portal.Client.Components.ScheduleComponents;

public partial class ScheduleWeekDay
{
    [Parameter]
    public required JobTypeEnum JobType { get; set; }

    [Parameter]
    public required DateOnly Day { get; set; }

    [Parameter]
    public required List<WeeklyScheduleDto> Items { get; set; }

    [Parameter]
    public EventCallback OnUpdate { get; set; }

    private TabTypeEnum _selectedtab = TabTypeEnum.ByTrack;
    private Dictionary<int, List<WeeklyScheduleDto>> _scheduleGroupedByTrack = [];

    protected override async Task OnParametersSetAsync() => _scheduleGroupedByTrack = GetItemsOrderedByTrack();

    private Dictionary<int, List<WeeklyScheduleDto>> GetItemsOrderedByTrack()
    {
        Dictionary<int, List<WeeklyScheduleDto>> itemsByTrack = Items
            .GroupBy(x => x.ScheduleTrackId)
            .ToDictionary(x => x.Key, x => x.OrderBy(x => x.Schedule.Start).ToList());
        return itemsByTrack;
    }

    private async Task OpenDialogAsync(WeeklyScheduleDto cal)
    {
        CustomCalendarItem calendarItem = new()
        {
            Colour = cal.Schedule.Colour.ColourHex,
            ColourId = cal.Schedule.Colour.ScheduleColourId,
            Start = DateTimeBuilder.cal.Schedule.Start,

        };

        DialogParameters parameter = new DialogParameters<CustomCalendarItem> { { "CalendarItem", calendarItem } };
        DialogOptions options = new() { CloseButton = false, CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large };

        IDialogReference res = await _dialog.ShowAsync<ViewEditSchedule>("", parameter, options);
        int? returnVal = await res.GetReturnValueAsync<int?>();
        if (returnVal is not null)
        {
            await OnUpdate.InvokeAsync();
        }
    }
}