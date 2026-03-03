using GoogleMapsComponents;
using GoogleMapsComponents.Maps;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Client.Webmodels;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Job;

public partial class Job : IDisposable
{
    [Parameter]
    public required int JobId { get; set; }

    private JobDetailsDto? _job;
    private List<JobNoteDto> _notes = [];
    private DummyJobData _dummyData = new();
    private AdvancedGoogleMap? _map;
    private MapOptions _mapOptions = default!;

    private readonly List<MarkerData> Markers = [];

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await base.OnInitializedAsync();
        await LoadJobData();
        await LoadNotes();
        LatLngLiteral center = new(-37.8136, 144.9631);
        if (_job?.Address?.LatLng is not null)
        {
            center = new LatLngLiteral(_job.Address.LatLng.Latitude, _job.Address.LatLng.Longitude);
            Markers.Add(new MarkerData
            {
                Id = 1,
                Lat = _job.Address.LatLng.Latitude,
                Lng = _job.Address.LatLng.Longitude,
                Title = "Job",
                Draggable = false
            });
        }
        _mapOptions = new MapOptions
        {
            Zoom = 13,
            Center = center,
            MapTypeId = MapTypeId.Roadmap,
            MapId = "Single_map_id"
        };
        IsLoading = false;
    }

    private async Task LoadJobData()
    {
        try
        {
            Result<JobDetailsDto>? result = await _apiService.Job(JobId);
            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                _job = result.Value;
            }
            else
            {
                _snackbar?.Add("Error loading job details", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            _snackbar?.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private string GetAddressString()
    {
        if (_job?.Address is null)
            return "No address";

        return $"{_job.Address.Suburb.ToUpper()}, {_job.Address.State} {_job.Address.PostCode}";
    }

    private async Task LoadNotes()
    {
        if (JobId <= 0) return;
        try
        {
            Result<List<JobNoteDto>>? result = await _apiService.GetJobNotes(JobId);
            if (result is not null && result.IsSuccess && result.Value is not null)
                _notes = result.Value;
            else
                _notes = [];
        }
        catch
        {
            _notes = [];
        }
        StateHasChanged();
    }

    private async Task HandleNotesRefreshed()
    {
        await LoadNotes();
    }

    private Task HandleAddSiteVisit()
    {
        // TODO: Implement add site visit functionality
        return Task.CompletedTask;
    }

    public void Dispose() => _map?.DisposeAsync();

    private class DummyJobData
    {
        public int TasksCount { get; set; } = 0;
        public int InvoicesCount { get; set; } = 0;
        public int ContactsCount { get; set; } = 0;
        public string ContactName { get; set; } = "ALL HOMES / PERRY";
        public string Phone { get; set; } = "018 368 142";
        public string Council { get; set; } = "SHIRE OF MACEDON RANGES";
        public string PlanRef { get; set; } = "LP1727490";

        public List<TechnicalContact> TechnicalContacts { get; set; } = [];
        public List<TaskItem> Tasks { get; set; } = [];
        public List<InvoiceItem> Invoices { get; set; } = [];
        public List<ChecklistItem> ChecklistItems { get; set; } =
    [
        new ChecklistItem { Name = "RE Survey", Checked = false, Sent = false },
        new ChecklistItem { Name = "Feature Plan", Checked = false, Sent = false },
        new ChecklistItem { Name = "MGA", Checked = false, Sent = false }
    ];
        public List<SiteVisit> SiteVisits { get; set; } = [];
        public List<ActivityItem> Activities { get; set; } =
    [
        new ActivityItem { Title = "Note Added", Time = "2 hours ago" },
        new ActivityItem { Title = "Job Modified", Time = "Yesterday" }
    ];
    }

    private class TechnicalContact
    {
        public string Role { get; set; } = "";
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
    }

    private class TaskItem
    {
        public string Name { get; set; } = "";
        public string QuotedPrice { get; set; } = "";
        public string ActiveDate { get; set; } = "";
        public string CompletedDate { get; set; } = "";
    }

    private class InvoiceItem
    {
        public string Number { get; set; } = "";
        public string Contact { get; set; } = "";
        public string TotalPrice { get; set; } = "";
        public string CreatedDate { get; set; } = "";
    }

    private class ChecklistItem
    {
        public string Name { get; set; } = "";
        public bool Checked { get; set; }
        public bool Sent { get; set; }
    }

    private class SiteVisit
    {
        public string Date { get; set; } = "";
        public string Details { get; set; } = "";
    }

    private class ActivityItem
    {
        public string Title { get; set; } = "";
        public string Time { get; set; } = "";
    }
}