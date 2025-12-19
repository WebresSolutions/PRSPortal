using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Client.Webmodels;
using Portal.Shared;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages;

public partial class Schedule
{
    // Add your scheduling logic here
    [SupplyParameterFromQuery]
    [Parameter]
    public DateOnly? Date { get; set; }

    [SupplyParameterFromQuery]
    [Parameter]
    public int? JobType { get; set; }

    private DateTime DateTime { get; set; }

    private JobTypeEnum JobTypeEnum;

    private List<ScheduleSlotDtoWithCalendar> scheduleSlots = [];
    /// <summary>
    /// Called when the component is initialized.
    /// Data loading for the grid is now handled by LoadFacilitiesServerData.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    protected override async Task OnInitializedAsync()
    {
        base.IsLoading = true;
        await base.OnInitializedAsync();
        Date ??= DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2).AddDays(-3));
        DateTime = Date.Value.ToDateTime(new TimeOnly(0, 0));
        JobTypeEnum = JobType is null ? JobTypeEnum.Construction : (JobTypeEnum)JobType;
        await LoadSchedule();
        base.IsLoading = false;
    }

    private async Task LoadSchedule()
    {
        base.IsLoading = true;
        Result<List<ScheduleSlotDTO>> res = await _apiService.GetIndividualSchedule(Date!.Value, JobTypeEnum);
        if (res.Error is null && res.Value is not null)
        {
            scheduleSlots = [.. res.Value.Select(x =>
                new ScheduleSlotDtoWithCalendar()
                {
                    Day = x.Day,
                    SlotId = x.SlotId,
                    Schedule = x.Schedule,
                    AssignedUsers = x.AssignedUsers,
                    Events = []
                })];

            foreach (ScheduleSlotDtoWithCalendar slot in scheduleSlots)
                slot.SetEvents();
        }
        else
        {
            _snackbar.Add(res.ErrorDescription ?? "Error occured while loading the schedules", Severity.Error);
        }
        base.IsLoading = false;
    }

    private async Task ReloadCalendar(DateTime dateTime)
    {
        DateTime = dateTime;
        Date = DateOnly.FromDateTime(dateTime);
        await LoadSchedule();
    }

    private async Task SwapJobType()
    {
        JobTypeEnum = JobTypeEnum == JobTypeEnum.Construction ? JobTypeEnum.Surveying : JobTypeEnum.Construction;
        await LoadSchedule();
    }

    private async Task AddNewSchedule(int trackId)
    {

    }
}