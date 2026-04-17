using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Client.Components.TimeSheet;
using Portal.Shared.DTO.Job;
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
    private const int _currentUserId = 0;

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

    /// <summary>
    /// Initializes the component with today's date and loads time sheet entries for the current week.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        _breadCrumbService.SetBreadCrumbItems(
          [
            new("Time Sheets", href: "/timesheets", disabled: true),
          ]);
        await base.OnInitializedAsync();
        SelectedDate = DateOnly.FromDateTime(DateTime.Today);
        await LoadEntriesAsync();
        IsLoading = false;
    }

    /// <summary>
    /// Gets the Monday of the week containing the given date.
    /// </summary>
    /// <param name="date">The date within the week.</param>
    /// <returns>The week start (Monday) for that date.</returns>
    private static DateOnly GetWeekStart(DateOnly date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }

    /// <summary>
    /// Loads time sheet entries for the current week from the API and updates the UI.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadEntriesAsync()
    {
        StateHasChanged();
        try
        {
            DateTime start = _weekStart.ToDateTime(TimeOnly.MinValue);
            DateTime end = _weekStart.AddDays(7).ToDateTime(TimeOnly.MinValue);
            Result<TimeSheetDto[]> result = await _apiService.GetUserTimeSheets(_currentUserId, start, end);
            if (result.IsSuccess && result.Value is not null)
                _entries = [.. result.Value.Select(x => new TimeSheetDto(x.Id, x.TypeId, x.Start.ToLocalTime(), x.End?.ToLocalTime(), x.UserId, x.JobId, x.Description, "", x.JobNumber))];
            else
                _entries = [];
        }
        finally
        {
            StateHasChanged();
        }
    }

    /// <summary>
    /// Sets the selected day and refreshes the UI.
    /// </summary>
    /// <param name="date">The date to select.</param>
    private void SelectDay(DateOnly date)
    {
        SelectedDate = date;
        StateHasChanged();
    }

    /// <summary>
    /// Handles date picker changes; loads entries for the new week if the week changed and updates the selected date.
    /// </summary>
    /// <param name="value">The new date/time value from the picker.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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

    /// <summary>
    /// Calculates the duration in hours for a time sheet entry.
    /// </summary>
    /// <param name="e">The time sheet entry.</param>
    /// <returns>Total hours between start and end (or 1 hour if end is null).</returns>
    private static double GetEntryHours(TimeSheetDto e)
    {
        DateTime end = e.End ?? e.Start.AddHours(1);
        return (end - e.Start).TotalHours;
    }

    /// <summary>
    /// Formats a number of hours for display (e.g. "1" or "2.5").
    /// </summary>
    /// <param name="hours">The hours value.</param>
    /// <returns>A culture-invariant string representation.</returns>
    private static string FormatHours(double hours) => hours.ToString("0.#", CultureInfo.InvariantCulture);

    /// <summary>
    /// Opens the add/edit time sheet entry dialog with optional default or existing entry.
    /// </summary>
    /// <param name="defaultTimeSheet">Existing entry to edit, or null to add a new entry.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task OpenAddEntryDialog(TimeSheetDto? defaultTimeSheet)
    {
        DialogParameters parameters = new()
        {
            ["SelectedDate"] = SelectedDate,
            ["OnSaved"] = EventCallback.Factory.Create(this, LoadEntriesAsync),
            ["EditEntry"] = defaultTimeSheet,
            [nameof(AddTimeSheetEntryDialog.Job)] = defaultTimeSheet is not null ? new JobDetailsDto() { JobId = defaultTimeSheet.JobId ?? 0, JobNumber = defaultTimeSheet.JobNumber ?? "" } : null
        };
        DialogOptions options = new() { CloseOnEscapeKey = true };
        await _dialog.ShowAsync<AddTimeSheetEntryDialog>("", parameters, options);
    }

    /// <summary>
    /// Stops the timer for the given entry by setting its end time to now and updating via the API.
    /// </summary>
    /// <param name="entry">The time sheet entry to stop.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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

    /// <summary>
    /// Prompts for confirmation and deletes the specified time sheet entry, then reloads entries on success.
    /// </summary>
    /// <param name="entry">The time sheet entry to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task DeleteEntry(TimeSheetDto entry)
    {
        bool? confirm = await _dialog.ShowMessageBoxAsync(
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