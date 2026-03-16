using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Client.Webmodels;
using Portal.Shared;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Schedule;

/// <summary>
/// Blazor page component for displaying and managing schedules
/// Shows schedule slots for a specific date and job type with calendar integration
/// </summary>
public partial class ScheduleDayView
{
    /// <summary>
    /// Gets or sets the navigation manager for page navigation
    /// </summary>
    [Inject]
    private NavigationManager? NavigationManager { get; set; }

    /// <summary>
    /// Gets or sets the date to display schedules for, supplied from route parameters (format: yyyy-MM-dd)
    /// </summary>
    [Parameter]
    public string? Date { get; set; }

    /// <summary>
    /// Gets or sets the job type filter, supplied from route parameters
    /// </summary>
    [Parameter]
    public int? JobType { get; set; }

    private UserDto[] _users = [];

    /// <summary>
    /// Gets or sets the date/time value for calendar display
    /// </summary>
    private DateTime _dateTime;

    /// <summary>
    /// Gets or sets the job type enum value
    /// </summary>
    private JobTypeEnum _jobTypeEnum = JobTypeEnum.Surveying;

    /// <summary>
    /// Gets or sets the list of schedule slots with calendar event information
    /// </summary>
    private List<ScheduleTrackDtoWithCalendar> _scheduleSlots = [];
    /// <summary>
    /// Called when the component is initialized.
    /// Data loading for the grid is now handled by LoadFacilitiesServerData.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    protected override async Task OnInitializedAsync()
    {
        base.IsLoading = true;
        await base.OnInitializedAsync();

        Result<UserDto[]> users = await _apiService.GetUsersList();
        if (users.IsSuccess && users.Value is not null)
            _users = users.Value;
        else
            _snackbar.Add(users.ErrorDescription ?? "Error occured while loading the users", Severity.Error);

        // Parse date from route parameter or default to today
        if (string.IsNullOrEmpty(Date) || !DateOnly.TryParse(Date, out DateOnly dateOnly))
        {
            dateOnly = DateOnly.FromDateTime(System.DateTime.Today);
            Date = dateOnly.ToString("yyyy-MM-dd");
        }

        // If no job type provided, default to Construction
        JobType ??= (int)JobTypeEnum.Construction;

        _dateTime = dateOnly.ToDateTime(new TimeOnly(0, 0));
        _jobTypeEnum = (JobTypeEnum)JobType.Value;

        // Navigate to URL with parameters if they weren't provided
        if (NavigationManager != null)
        {
            string currentUri = NavigationManager.Uri;
            string expectedUri = $"/schedule/{Date}/{JobType.Value}";
            if (!currentUri.Contains(expectedUri))
            {
                NavigationManager.NavigateTo(expectedUri);
                return;
            }
        }

        //await LoadSchedule();
        base.IsLoading = false;
    }

    /// <summary>
    /// Called when component parameters (Date, JobType) are set or changed; updates internal state and loads schedule data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        // Update internal state when parameters change
        if (!string.IsNullOrEmpty(Date) && JobType.HasValue)
        {
            if (DateOnly.TryParse(Date, out DateOnly dateOnly))
            {
                _dateTime = dateOnly.ToDateTime(new TimeOnly(0, 0));
                _jobTypeEnum = (JobTypeEnum)JobType.Value;
                await LoadSchedule();
            }
        }
    }

    /// <summary>
    /// Loads schedule slots for the current date and job type from the server
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task LoadSchedule()
    {
        base.IsLoading = true;
        DateOnly dateOnly = DateOnly.FromDateTime(_dateTime);
        Result<List<ScheduleTrackDto>> res = await _apiService.GetIndividualSchedule(dateOnly, _jobTypeEnum);
        if (res.Error is null && res.Value is not null)
        {
            _scheduleSlots = [.. res.Value.Select(x =>
                new ScheduleTrackDtoWithCalendar()
                {
                    Day = x.Day,
                    TrackId = x.TrackId,
                    Schedule = x.Schedule,
                    AssignedUsers = x.AssignedUsers,
                    Events = []
                })];

            foreach (ScheduleTrackDtoWithCalendar slot in _scheduleSlots)
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
    /// <param name="dateOnly">The new date to load schedules for</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private void ReloadCalendar(DateOnly dateOnly)
    {
        if (NavigationManager != null && JobType.HasValue)
            NavigationManager.NavigateTo($"/schedule/{dateOnly:yyyy-MM-dd}/{JobType.Value}");
    }

    /// <summary>
    /// Switches between Construction and Surveying job types and reloads schedules
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    private void SwapJobType()
    {
        JobTypeEnum newJobType = _jobTypeEnum == JobTypeEnum.Construction ? JobTypeEnum.Surveying : JobTypeEnum.Construction;
        if (NavigationManager != null && !string.IsNullOrEmpty(Date))
        {
            NavigationManager.NavigateTo($"/schedule/{Date}/{(int)newJobType}");
        }
    }

    public async Task NewTrack()
    {
        DateOnly date = DateOnly.FromDateTime(_dateTime.Date);
        UpdateScheduleTrackDto track = new() { Date = date, ScheduleTrackId = 0, JobTypeEnum = (JobTypeEnum)JobType! };

        Result<ScheduleTrackDto> result = await _apiService.UpdateScheduleTrack(track);
        if (result.IsSuccess)
            await LoadSchedule();
        else
            _snackbar.Add("Failed to create new schedule track", Severity.Error);
    }

    /// <summary>
    /// Navigates back to the previous page
    /// </summary>
    private void NavigateBack()
    {
        NavigationManager?.NavigateTo("/");
    }
}