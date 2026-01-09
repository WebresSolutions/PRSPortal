using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Client.Webmodels;
using Portal.Shared;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.ResponseModels;
using Microsoft.AspNetCore.Components.Web;

namespace Portal.Client.Pages;

/// <summary>
/// Blazor page component for displaying and managing schedules
/// Shows schedule slots for a specific date and job type with calendar integration
/// </summary>
public partial class Schedule
{
    /// <summary>
    /// Gets or sets the navigation manager for page navigation
    /// </summary>
    [Inject]
    private NavigationManager? NavigationManager { get; set; }
    /// <summary>
    /// Gets or sets the date to display schedules for, supplied from query parameters
    /// </summary>
    [SupplyParameterFromQuery]
    [Parameter]
    public DateOnly? Date { get; set; }

    /// <summary>
    /// Gets or sets the job type filter, supplied from query parameters
    /// </summary>
    [SupplyParameterFromQuery]
    [Parameter]
    public int? JobType { get; set; }

    /// <summary>
    /// Gets or sets the date/time value for calendar display
    /// </summary>
    private DateTime DateTime { get; set; }

    /// <summary>
    /// Gets or sets the job type enum value
    /// </summary>
    private JobTypeEnum JobTypeEnum;

    /// <summary>
    /// Gets or sets the list of schedule slots with calendar event information
    /// </summary>
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
        Date ??= DateOnly.FromDateTime(new DateTime(2025, 10, 10));
        DateTime = Date.Value.ToDateTime(new TimeOnly(0, 0));
        JobTypeEnum = JobType is null ? JobTypeEnum.Construction : (JobTypeEnum)JobType;
        await LoadSchedule();
        base.IsLoading = false;
    }

    /// <summary>
    /// Loads schedule slots for the current date and job type from the server
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
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

    /// <summary>
    /// Reloads the calendar with a new date
    /// </summary>
    /// <param name="dateTime">The new date/time to load schedules for</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task ReloadCalendar(DateTime dateTime)
    {
        DateTime = dateTime;
        Date = DateOnly.FromDateTime(dateTime);
        await LoadSchedule();
    }

    /// <summary>
    /// Switches between Construction and Surveying job types and reloads schedules
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task SwapJobType()
    {
        JobTypeEnum = JobTypeEnum == JobTypeEnum.Construction ? JobTypeEnum.Surveying : JobTypeEnum.Construction;
        await LoadSchedule();
    }

    /// <summary>
    /// Adds a new schedule entry for the specified track
    /// </summary>
    /// <param name="trackId">The schedule track identifier</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task AddNewSchedule(int trackId)
    {

    }

    /// <summary>
    /// Navigates back to the previous page
    /// </summary>
    private void NavigateBack()
    {
        NavigationManager?.NavigateTo("/");
    }
}