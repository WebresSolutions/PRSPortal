using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Components.ScheduleComponents;

public partial class AddSchedule
{
    [Parameter]
    public required int TrackId { get; set; }

    private TimeSpan _timeFrom;
    private TimeSpan _timeTo;
    private UpdateScheduleDto _UpdateScheduleDto = null!;
    private ListJobDto? _selectedJob;
    private ScheduleColourDto _selectedScheduleColour = null!;
    private List<ScheduleColourDto> _colours = [];

    protected override async Task OnParametersSetAsync()
    {
        base.IsLoading = true;

        _timeFrom = new TimeOnly(8, 0, 0).ToTimeSpan();
        _timeTo = new TimeOnly(9, 0, 0).ToTimeSpan();

        Result<List<ScheduleColourDto>> colours = await _apiService.GetScheduleColours();
        if (colours.IsSuccess)
        {
            _colours = colours.Value!;
            _selectedScheduleColour = _colours.First();
        }

        _UpdateScheduleDto = new()
        {
            Start = new TimeOnly(8, 0, 0),
            End = new TimeOnly(10, 0, 0),
            ColourId = _selectedScheduleColour.ScheduleColourId,
            Notes = string.Empty,
            TrackId = TrackId,
            Id = 0,
            JobId = null
        };

        base.IsLoading = false;
    }

    private async Task<IEnumerable<ListJobDto>> GetJobsFromSearch(string search)
    {
        JobFilterDto filter = new() { AddressSearch = search, Page = 1, PageSize = 50 };

        Result<PagedResponse<ListJobDto>> jobs = await _apiService.GetAllJobs(filter);
        if (jobs.IsSuccess && jobs.Value is not null)
        {
            return jobs.Value.Result;
        }
        else
        {
            _snackbar.Add("Failed to load jobs for autocomplete.", Severity.Error);
            return [];
        }
    }

    private async Task OnSave()
    {
        if (_timeFrom.TotalMinutes > _timeTo.TotalMinutes)
        {
            _snackbar.Add("Start must be before end", Severity.Warning);
            return;
        }
        _UpdateScheduleDto.Start = new TimeOnly(_timeFrom.Hours, _timeFrom.Minutes, 0);
        _UpdateScheduleDto.End = new TimeOnly(_timeTo.Hours, _timeTo.Minutes, 0);
        _UpdateScheduleDto.ColourId = _selectedScheduleColour.ScheduleColourId;

        Result<int> res = await _apiService.UpdateSchedule(_UpdateScheduleDto);
        if (res.IsSuccess)
        {
            MudDialog?.Close(DialogResult.Ok(res.Value));
        }
        else
            _snackbar.Add("Failed to save schedule", Severity.Error);
    }

    private void OnJobSelected(ListJobDto? job)
    {
        _UpdateScheduleDto.JobId = job?.JobId;
        _selectedJob = job;
    }
}