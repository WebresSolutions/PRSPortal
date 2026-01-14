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

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadCouncilData();
    }

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

    private async Task LoadJobs(int page)
    {
        IsLoading = true;
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
            IsLoading = false;
        }
    }

    private string GetAddressString()
    {
        if (_council?.address is null)
            return "No address";

        List<string> parts = [];
        if (!string.IsNullOrWhiteSpace(_council.address.suburb))
            parts.Add(_council.address.suburb.ToUpper());
        if (!string.IsNullOrWhiteSpace(_council.address.State.ToString()))
            parts.Add(_council.address.State.ToString());
        if (!string.IsNullOrWhiteSpace(_council.address.postCode))
            parts.Add(_council.address.postCode);

        return parts.Count > 0 ? string.Join(" ", parts) : "No address";
    }
}