using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Components.ScheduleComponents;

public partial class ViewEditSchedule
{
    [Parameter]
    public required int ScheduleId { get; set; }

    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }

    private bool _isEditing = false;
    private bool _loading = true;
    private string? _errorMessage;
    private ScheduleDto? _schedule;
    private TimeSpan _timeFrom;
    private TimeSpan _timeTo;
    private UpdateScheduleDto _UpdateScheduleDto = null!;
    private ListJobDto? _selectedJob;
    private ScheduleColourDto _selectedScheduleColour = null!;
    private List<ScheduleColourDto> _colours = [];

    protected override async Task OnInitializedAsync()
    {
        Result<ScheduleDto> scheduleResult = await _apiService.GetSchedule(ScheduleId);
        Result<List<ScheduleColourDto>> coloursResult = await _apiService.GetScheduleColours();

        if (!scheduleResult.IsSuccess)
        {
            _errorMessage = scheduleResult.ErrorDescription ?? "Failed to load schedule.";
            _loading = false;
            return;
        }

        _schedule = scheduleResult.Value;
        if (_schedule is null)
        {
            _errorMessage = "Schedule not found.";
            _loading = false;
            return;
        }

        _timeFrom = new TimeSpan(_schedule.Start.Hour, _schedule.Start.Minute, 0);
        _timeTo = new TimeSpan(_schedule.End.Hour, _schedule.End.Minute, 0);

        _UpdateScheduleDto = new()
        {
            Start = _schedule.Start,
            End = _schedule.End,
            ColourId = _schedule.Colour.ScheduleColourId,
            Notes = _schedule.Description,
            TrackId = _schedule.ScheduleTrackId ?? 0,
            Id = _schedule.ScheduleId,
            JobId = _schedule.Job?.JobId
        };

        if (coloursResult.IsSuccess && coloursResult.Value is not null)
        {
            _colours = coloursResult.Value;
            _selectedScheduleColour = _colours.FirstOrDefault(x => x.ScheduleColourId == _UpdateScheduleDto.ColourId)
                ?? _colours.First();
        }

        // Pre-populate the job dropdown when the schedule has an assigned job
        if (_schedule?.Job is not null)
        {
            _selectedJob = new ListJobDto
            {
                JobId = _schedule.Job.JobId,
                JobNumber = _schedule.Job.JobNumber,
                Address = _schedule.Job.Address ?? new AddressDTO()
            };
        }

        _loading = false;
    }

    private string JobAddressDisplay => _schedule?.Job?.Address?.ToDisplayString() ?? string.Empty;

    private async Task<IEnumerable<ListJobDto>> GetJobsFromSearch(string search)
    {

        JobFilterDto filter;
        if (int.TryParse(search, out int searchInt))
            filter = new() { JobNumberSearch = search, Page = 1, PageSize = 50 };
        else
            filter = new() { AddressSearch = search, Page = 1, PageSize = 50 };

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
