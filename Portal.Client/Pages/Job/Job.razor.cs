using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Job;

public partial class Job
{
    [Parameter]
    public required int JobId { get; set; }

    [Parameter]
    public bool IsEditing { get; set; }

    private JobDetailsDto? _job;
    private DummyJobData _dummyData = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadJobData();
    }

    private async Task LoadJobData()
    {
        IsLoading = true;
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
        finally
        {
            IsLoading = false;
        }
    }

    private string GetAddressString()
    {
        if (_job?.Address is null)
            return "No address";

        return $"{_job.Address.suburb.ToUpper()}, {_job.Address.State} {_job.Address.postCode}";
    }

    private Task HandleAddNote()
    {
        // TODO: Implement add note functionality
        return Task.CompletedTask;
    }

    private Task HandleAddSiteVisit()
    {
        // TODO: Implement add site visit functionality
        return Task.CompletedTask;
    }

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