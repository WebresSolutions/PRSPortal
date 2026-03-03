using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared;
using Portal.Shared.DTO.Councils;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Council;

public partial class Council
{
    [Parameter]
    public required int CouncilId { get; set; }

    private CouncilDetailsDto? _council;
    private PagedResponse<ListJobDto>? _pagedJobs;
    private readonly int _rowsPerPage = 15;
    private int _currentPage = 1;

    /// <summary>
    /// Initializes the component and loads council details and the first page of jobs.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        IsLoading = true;

        await LoadCouncilData();
        IsLoading = false;
    }

    /// <summary>
    /// Loads the council details and first page of jobs from the API for the current council.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadCouncilData()
    {
        IsLoading = true;
        try
        {
            // Load council details
            Result<CouncilDetailsDto>? result = await _apiService.GetCouncilDetails(CouncilId);
            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                _council = result.Value;
            }
            else
            {
                _snackbar?.Add("Error loading council details", Severity.Error);
            }

            // Load jobs separately with pagination
            await LoadJobs(_currentPage);
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

    /// <summary>
    /// Loads a page of jobs associated with the current council from the API.
    /// </summary>
    /// <param name="page">The page number to load.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadJobs(int page)
    {
        try
        {
            Result<PagedResponse<ListJobDto>>? jobsResult = await _apiService.GetCouncilJobs(CouncilId, page, _rowsPerPage, SortDirectionEnum.Desc, null);
            if (jobsResult is not null && jobsResult.IsSuccess && jobsResult.Value is not null)
            {
                _pagedJobs = jobsResult.Value;
                _currentPage = page;
            }
            else
            {
                _snackbar?.Add("Error loading council jobs", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            _snackbar?.Add($"Error loading jobs: {ex.Message}", Severity.Error);
        }
        finally
        {
        }
    }

    /// <summary>
    /// Returns a formatted address string (suburb, state, post code) for the current council.
    /// </summary>
    /// <returns>The formatted address or "No address" if none is set.</returns>
    private string GetAddressString() => _council?.address?.ToDisplayString() ?? "No address";
}