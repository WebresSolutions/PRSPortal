using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Job;

public partial class Job
{
    [Parameter]
    public required int JobId { get; set; }

    private JobDetailsDto? _job;

    /// <summary>
    /// Initializes the component by loading job details, notes, and configuring the map.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await base.OnInitializedAsync();
        await LoadJobData();
        _breadCrumbService.SetBreadCrumbItems(
           [
                new("Jobs", href: "/jobs", disabled: false),
                new($"{_job?.JobNumber ?? "Job"}", href: $"/jobs/{JobId}", disabled: true)
           ]);
        IsLoading = false;
    }

    /// <summary>
    /// Loads the job details from the API for the current job ID.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadJobData()
    {
        try
        {
            Result<JobDetailsDto>? result = await _apiService.Job(JobId);
            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                _job = result.Value;
                StateHasChanged();
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

    /// <summary>
    /// Handles the add site visit action. Reserved for future implementation.
    /// </summary>
    /// <returns>A completed task.</returns>
    private Task HandleAddSiteVisit()
    {
        // TODO: Implement add site visit functionality
        return Task.CompletedTask;
    }
}
