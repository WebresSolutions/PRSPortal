using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Client.Components;
using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.ResponseModels;
using System.Globalization;

namespace Portal.Client.Pages;

public partial class TimeSheets
{
    /// <summary>
    /// Gets or sets the date to display schedules for, supplied from route parameters (format: yyyy-MM-dd)
    /// </summary>
    [Parameter]
    public string? Date { get; set; }

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
        IsLoading = true;
        await base.OnInitializedAsync();
        SelectedDate = DateOnly.FromDateTime(DateTime.Today);
        await LoadEntriesAsync();
        IsLoading = false;
    }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }

    private async Task LoadEntriesAsync()
    {
        StateHasChanged();
        try
        {
            DateTime start = _weekStart.ToDateTime(TimeOnly.MinValue);
            DateTime end = _weekStart.AddDays(7).ToDateTime(TimeOnly.MinValue);
            Result<TimeSheetDto[]> result = await _apiService.GetUserTimeSheets(CurrentUserId, start, end);
            if (result.IsSuccess && result.Value is not null)
                _entries = [.. result.Value.Select(x => new TimeSheetDto(x.Id, x.Start.ToLocalTime(), x.End?.ToLocalTime(), x.UserId, x.JobId, x.Description, ""))];
            else
                _entries = [];
        }
        finally
        {
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

    private static double GetEntryHours(TimeSheetDto e)
    {
        DateTime end = e.End ?? e.Start.AddHours(1);
        return (end - e.Start).TotalHours;
    }

    private static string FormatHours(double hours) => hours.ToString("0.#", CultureInfo.InvariantCulture);

    private async Task OpenAddEntryDialog(TimeSheetDto? defaultTimeSheet)
    {
        DialogParameters parameters = new()
        {
            ["SelectedDate"] = SelectedDate,
            ["OnSaved"] = EventCallback.Factory.Create(this, LoadEntriesAsync),
            ["EditEntry"] = defaultTimeSheet
        };
        DialogOptions options = new() { CloseOnEscapeKey = true };
        await _dialog.ShowAsync<AddTimeSheetEntryDialog>("Add time entry", parameters, options);
    }

    private async Task StopEntry(TimeSheetDto entry)
    {
        TimeSheetDto updatedEntry = entry with { End = DateTime.Now };
        Result<TimeSheetDto> result = await _apiService.UpdateTimeSheet(updatedEntry);
        if (result.IsSuccess)
        {
            _snackbar.Add("Timer Stopped.", Severity.Success);
            await LoadEntriesAsync();
        }
        else
            _snackbar.Add(result.ErrorDescription ?? "Failed to add entry.", Severity.Error);
    }

    private async Task DeleteEntry(TimeSheetDto entry)
    {
        bool? confirm = await _dialog.ShowMessageBox(
            "Confirm Delete",
            "Are you sure you want to delete this time entry?",
            yesText: "Delete",
            cancelText: "Cancel",
            options: new DialogOptions { CloseOnEscapeKey = true });
        if (confirm == true)
        {
            Result<bool> result = await _apiService.DeleteTimeSheetEntry(entry);
            if (result.IsSuccess)
            {
                _snackbar.Add("Entry deleted.", Severity.Success);
                await LoadEntriesAsync();
            }
            else
                _snackbar.Add(result.ErrorDescription ?? "Failed to delete entry.", Severity.Error);
        }
    }
}