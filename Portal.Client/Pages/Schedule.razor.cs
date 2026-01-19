using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Client.Components.ScheduleComponents;
using Portal.Client.Webmodels;
using Portal.Shared;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.ResponseModels;

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
    /// Gets or sets the date to display schedules for, supplied from route parameters (format: yyyy-MM-dd)
    /// </summary>
    [Parameter]
    public string? Date { get; set; }

    /// <summary>
    /// Gets or sets the job type filter, supplied from route parameters
    /// </summary>
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

        // Parse date from route parameter or default to today
        if (string.IsNullOrEmpty(Date) || !DateOnly.TryParse(Date, out DateOnly dateOnly))
        {
            dateOnly = DateOnly.FromDateTime(DateTime.Today);
            Date = dateOnly.ToString("yyyy-MM-dd");
        }

        // If no job type provided, default to Construction
        JobType ??= (int)JobTypeEnum.Construction;

        DateTime = dateOnly.ToDateTime(new TimeOnly(0, 0));
        JobTypeEnum = (JobTypeEnum)JobType.Value;

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

    private async Task OpenDialogAsync(CustomCalendarItem cal)
    {
        Console.WriteLine("Opening dialog for calendar item: " + cal?.Text);

        if (cal == null)
        {
            Console.WriteLine("Calendar item is null!");
            return;
        }

        DialogParameters parameter = new DialogParameters<CustomCalendarItem> { { "CalendarItem", cal } };
        DialogOptions options = new() { CloseButton = true, CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large };

        IDialogReference _ = await _dialog.ShowAsync<ViewScheduleIndividualDialog>("", parameter, options);
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        // Update internal state when parameters change
        if (!string.IsNullOrEmpty(Date) && JobType.HasValue)
        {
            if (DateOnly.TryParse(Date, out DateOnly dateOnly))
            {
                DateTime = dateOnly.ToDateTime(new TimeOnly(0, 0));
                JobTypeEnum = (JobTypeEnum)JobType.Value;
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
        DateOnly dateOnly = DateOnly.FromDateTime(DateTime);
        Result<List<ScheduleSlotDTO>> res = await _apiService.GetIndividualSchedule(dateOnly, JobTypeEnum);
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
    /// <param name="dateOnly">The new date to load schedules for</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private void ReloadCalendar(DateOnly dateOnly)
    {
        if (NavigationManager != null && JobType.HasValue)
        {
            NavigationManager.NavigateTo($"/schedule/{dateOnly:yyyy-MM-dd}/{JobType.Value}");
        }
    }

    /// <summary>
    /// Switches between Construction and Surveying job types and reloads schedules
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    private void SwapJobType()
    {
        JobTypeEnum newJobType = JobTypeEnum == JobTypeEnum.Construction ? JobTypeEnum.Surveying : JobTypeEnum.Construction;
        if (NavigationManager != null && !string.IsNullOrEmpty(Date))
        {
            NavigationManager.NavigateTo($"/schedule/{Date}/{(int)newJobType}");
        }
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