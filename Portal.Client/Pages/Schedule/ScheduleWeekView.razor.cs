using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Schedule;

/// <summary>
/// Code-behind for the schedule week view. Loads weekly data and groups by day.
/// </summary>
public partial class ScheduleWeekView
{
    [Inject]
    private NavigationManager? NavigationManager { get; set; }

    /// <summary>Route: Monday of the week (yyyy-MM-dd). Optional, defaults to current week.</summary>
    [Parameter]
    public string? Date { get; set; }

    /// <summary>Route: job type (1 = Construction, 2 = Surveying). Optional.</summary>
    [Parameter]
    public int? JobType { get; set; }

    private JobTypeEnum _jobTypeEnum = JobTypeEnum.Construction;
    private DateOnly _weekStart; // Monday
    private DateOnly _weekEnd;   // Sunday
    private WeeklyScheduleDto[] _weekData = [];
    private Dictionary<DateOnly, List<WeeklyScheduleDto>> _byDay = new();

    protected override async Task OnInitializedAsync()
    {
        base.IsLoading = true;
        await base.OnInitializedAsync();

        if (!DateOnly.TryParse(Date, out DateOnly parsed) || string.IsNullOrEmpty(Date))
            parsed = GetMonday(DateOnly.FromDateTime(DateTime.Today));
        else
            parsed = GetMonday(parsed);

        JobType ??= (int)JobTypeEnum.Construction;
        _jobTypeEnum = (JobTypeEnum)JobType.Value;
        _weekStart = parsed;
        _weekEnd = _weekStart.AddDays(6);

        if (NavigationManager != null && (!NavigationManager.Uri.Contains("/schedule/week/") || !NavigationManager.Uri.Contains($"{_weekStart:yyyy-MM-dd}")))
        {
            NavigationManager.NavigateTo($"/schedule/week/{_weekStart:yyyy-MM-dd}/{JobType.Value}", replace: false);
        }

        await LoadWeek();
        base.IsLoading = false;
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        if (string.IsNullOrEmpty(Date) || !JobType.HasValue) return;
        if (DateOnly.TryParse(Date, out DateOnly d))
        {
            _weekStart = GetMonday(d);
            _weekEnd = _weekStart.AddDays(6);
            _jobTypeEnum = (JobTypeEnum)JobType.Value;
            await LoadWeek();
        }
    }

    private static DateOnly GetMonday(DateOnly date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }

    private async Task LoadWeek()
    {
        base.IsLoading = true;
        Result<WeeklyScheduleDto[]> res = await _apiService.GetWeeklySchedule(_jobTypeEnum, _weekStart);
        if (res.IsSuccess && res.Value != null)
        {
            _weekData = res.Value;
            _byDay = _weekData
                .GroupBy(x => DateOnly.FromDateTime(x.Schedule.Start))
                .ToDictionary(g => g.Key, g => g.ToList());
        }
        else
            _snackbar.Add(res.ErrorDescription ?? "Failed to load week schedule", Severity.Error);
        base.IsLoading = false;
    }

    private void PreviousWeek()
    {
        _weekStart = _weekStart.AddDays(-7);
        _weekEnd = _weekStart.AddDays(6);
        if (NavigationManager != null && JobType.HasValue)
            NavigationManager.NavigateTo($"/schedule/week/{_weekStart:yyyy-MM-dd}/{JobType.Value}");
    }

    private void NextWeek()
    {
        _weekStart = _weekStart.AddDays(7);
        _weekEnd = _weekStart.AddDays(6);
        if (NavigationManager != null && JobType.HasValue)
            NavigationManager.NavigateTo($"/schedule/week/{_weekStart:yyyy-MM-dd}/{JobType.Value}");
    }

    private void SwapJobType()
    {
        JobTypeEnum next = _jobTypeEnum == JobTypeEnum.Construction ? JobTypeEnum.Surveying : JobTypeEnum.Construction;
        if (NavigationManager != null)
            NavigationManager.NavigateTo($"/schedule/week/{_weekStart:yyyy-MM-dd}/{(int)next}");
    }

    private List<WeeklyScheduleDto> GetItemsForDay(DateOnly day)
    {
        return _byDay.TryGetValue(day, out var list) ? list : [];
    }

    private static string TimeRange(DateTime start, DateTime end) =>
        $"{start:HH:mm}–{end:HH:mm}";
}
