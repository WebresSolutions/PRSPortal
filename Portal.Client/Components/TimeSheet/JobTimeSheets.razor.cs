using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Components.TimeSheet;

public partial class JobTimeSheets
{
    [Parameter]
    public required IEnumerable<TimeSheetDto> TimeSheetsList { get; set; }

    /// <summary>Optional job id to pre-fill when adding a new time entry from a job page.</summary>
    [Parameter]
    public int? JobId { get; set; }

    /// <summary>Invoked after add, edit, or delete so the parent can refresh data (e.g. reload job).</summary>
    [Parameter]
    public EventCallback OnSaved { get; set; }

    double TotalHours = 0;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        TotalHours = TimeSheetsList.Where(x => x.End is not null).Sum(x => (x.End! - x.Start).Value.TotalHours);
    }

    private async Task OpenAddDialog()
    {
        DialogParameters parameters = new()
        {
            [nameof(AddTimeSheetEntryDialog.SelectedDate)] = DateOnly.FromDateTime(DateTime.Today),
            [nameof(AddTimeSheetEntryDialog.JobId)] = JobId,
            [nameof(AddTimeSheetEntryDialog.OnSaved)] = OnSaved
        };
        DialogOptions options = new() { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, CloseButton = true };
        await DialogService.ShowAsync<AddTimeSheetEntryDialog>("", parameters, options);
    }

    private async Task OpenEditDialog(TimeSheetDto entry)
    {
        DialogParameters parameters = new()
        {
            [nameof(AddTimeSheetEntryDialog.EditEntry)] = entry,
            [nameof(AddTimeSheetEntryDialog.OnSaved)] = OnSaved
        };
        DialogOptions options = new() { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, CloseButton = true };
        await DialogService.ShowAsync<AddTimeSheetEntryDialog>("", parameters, options);
    }

    private async Task DeleteEntry(TimeSheetDto entry)
    {
        Result<bool> result = await ApiService.DeleteTimeSheetEntry(entry);
        if (result.IsSuccess)
        {
            Snackbar.Add("Time entry removed.", Severity.Success);
            TimeSheetsList = TimeSheetsList.Where(x => x.Id != entry.Id).ToList();
            // await OnSaved.InvokeAsync();
        }
        else
        {
            Snackbar.Add(result.ErrorDescription ?? "Could not delete entry.", Severity.Error);
        }
    }

    private static string FormatDuration(DateTime start, DateTime end)
    {
        TimeSpan d = end - start;
        if (d.TotalHours < 1)
            return $"{(int)d.TotalMinutes} min";

        int h = (int)d.TotalHours;
        int m = d.Minutes;
        return m > 0 ? $"{h}h {m}m" : $"{h}h";
    }
}