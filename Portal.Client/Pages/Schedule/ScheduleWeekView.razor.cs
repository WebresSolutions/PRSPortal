using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Schedule;

/// <summary>
/// Code-behind for the schedule week view. Loads weekly data and groups by day.
/// </summary>
public partial class ScheduleWeekView
{
    /// <summary>Route: Monday of the week (yyyy-MM-dd). Optional, defaults to current week.</summary>
    [Parameter]
    public string? Date { get; set; }

    /// <summary>Route: job type (1 = Construction, 2 = Surveying). Optional.</summary>
    [Parameter]
    public int? JobType { get; set; } = 1;

    private JobTypeEnum _jobTypeEnum = JobTypeEnum.Construction;
    private DateOnly _weekStart; // Monday
    private DateOnly _weekEnd;   // Sunday
    private WeeklyGroupedByScheduleDto[] _weekData = [];
    private Dictionary<DateOnly, List<WeeklyScheduleDto>> _byDay = [];

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

        if (!_navigationManager.Uri.Contains("/week/") || !_navigationManager.Uri.Contains($"{_weekStart:yyyy-MM-dd}"))
            _navigationManager.NavigateTo($"/week/{_weekStart:yyyy-MM-dd}/{JobType.Value}", replace: false);

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
        Result<WeeklyGroupedByScheduleDto[]> res = await _apiService.GetWeeklySchedule(_jobTypeEnum, _weekStart);
        if (res.IsSuccess && res.Value != null)
        {
            _weekData = res.Value;
            _byDay = _weekData
                .GroupBy(x => x.Date)
                .ToDictionary(g => g.Key, f => f.SelectMany(x => x.Schedules).ToList());
        }
        else
            _snackbar.Add(res.ErrorDescription ?? "Failed to load week schedule", Severity.Error);

        base.IsLoading = false;
    }

    private void PreviousWeek()
    {
        _weekStart = _weekStart.AddDays(-7);
        _weekEnd = _weekStart.AddDays(6);
        if (_navigationManager != null && JobType.HasValue)
            _navigationManager.NavigateTo($"/week/{_weekStart:yyyy-MM-dd}/{JobType.Value}");
    }

    private void NextWeek()
    {
        _weekStart = _weekStart.AddDays(7);
        _weekEnd = _weekStart.AddDays(6);
        if (_navigationManager != null && JobType.HasValue)
            _navigationManager.NavigateTo($"/week/{_weekStart:yyyy-MM-dd}/{JobType.Value}");
    }

    private void SwapJobType()
    {
        JobTypeEnum next = _jobTypeEnum == JobTypeEnum.Construction ? JobTypeEnum.Surveying : JobTypeEnum.Construction;
        _navigationManager?.NavigateTo($"/week/{_weekStart:yyyy-MM-dd}/{(int)next}");
    }

    private List<WeeklyScheduleDto> GetItemsForDay(DateOnly day)
    {
        return _byDay.TryGetValue(day, out List<WeeklyScheduleDto>? list) ? list : [];
    }

}
