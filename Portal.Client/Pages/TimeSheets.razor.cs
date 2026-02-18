using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Client.Components;
using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.ResponseModels;
using System.Globalization;

namespace Portal.Client.Pages;

public partial class TimeSheets
{
    private DateOnly _selectedDate;
    private DateOnly _weekStart;
    private TimeSheetDto[] _entries = [];
    private const int CurrentUserId = 0;

    private DateOnly SelectedDate
    {
        get => _selectedDate;
        set
        {
            _selectedDate = value;
            _weekStart = GetWeekStart(value);
        }
    }

    private IEnumerable<DateOnly> WeekDays
        => Enumerable.Range(0, 7).Select(i => _weekStart.AddDays(i));

    private IReadOnlyList<TimeSheetDto> EntriesForSelectedDay
        => [.. _entries
        .Where(e => e.Start.Date == SelectedDate.ToDateTime(TimeOnly.MinValue).Date)
        .OrderBy(e => e.Start)];

    private double TotalHoursForSelectedDay => EntriesForSelectedDay.Sum(e => GetEntryHours(e));

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        SelectedDate = DateOnly.FromDateTime(DateTime.Today);
        await LoadEntriesAsync();
    }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }

    private async Task LoadEntriesAsync()
    {
        IsLoading = true;
        StateHasChanged();
        try
        {
            DateTime start = _weekStart.ToDateTime(TimeOnly.MinValue);
            DateTime end = _weekStart.AddDays(7).ToDateTime(TimeOnly.MinValue);
            Result<TimeSheetDto[]> result = await _apiService.GetUserTimeSheets(CurrentUserId, start, end);
            if (result.IsSuccess && result.Value is not null)
                _entries = [.. result.Value.Select(x => new TimeSheetDto(x.id, x.Start.ToLocalTime(), x.End?.ToLocalTime(), x.UserId, x.JobId, x.Description))];
            else
                _entries = [];
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private void SelectDay(DateOnly date)
    {
        SelectedDate = date;
        StateHasChanged();
    }

    private async Task OnDateChanged(DateTime? value)
    {
        if (value is null) return;
        DateOnly newWeekStart = GetWeekStart(DateOnly.FromDateTime(value.Value));
        if (newWeekStart != _weekStart)
        {
            _weekStart = newWeekStart;
            await LoadEntriesAsync();
        }
        SelectedDate = DateOnly.FromDateTime(value.Value);
        StateHasChanged();
    }

    private TimeSheetDto[] GetEntriesForHour(int hour)
    {
        DateTime dayStart = SelectedDate.ToDateTime(new TimeOnly(hour, 0));
        DateTime dayEnd = hour < 18 ? SelectedDate.ToDateTime(new TimeOnly(hour + 1, 0)) : dayStart.AddHours(1);
        return [.. EntriesForSelectedDay.Where(e =>
        {
            DateTime start = e.Start;
            DateTime end = e.End ?? start.AddHours(1);
            return start < dayEnd && end > dayStart;
        })];
    }

    private static double GetEntryHours(TimeSheetDto e)
    {
        DateTime end = e.End ?? e.Start.AddHours(1);
        return (end - e.Start).TotalHours;
    }

    private static string FormatHours(TimeSheetDto e)
    {
        return GetEntryHours(e).ToString("0.#", CultureInfo.InvariantCulture);
    }

    private static string FormatHours(double hours)
    {
        return hours.ToString("0.#", CultureInfo.InvariantCulture);
    }

    private async Task OpenAddEntryDialog()
    {
        DialogParameters parameters = new()
        {
            ["SelectedDate"] = SelectedDate,
            ["OnSaved"] = EventCallback.Factory.Create(this, LoadEntriesAsync)
        };
        DialogOptions options = new() { CloseOnEscapeKey = true };
        await _dialog.ShowAsync<AddTimeSheetEntryDialog>("Add time entry", parameters, options);
    }
}