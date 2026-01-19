using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Contact;

public partial class Contact
{
    [Parameter]
    public required int ContactId { get; set; }

    private ContactDetailsDto? _contact;
    private PagedResponse<ListJobDto>? _pagedJobs;
    private readonly int _rowsPerPage = 15;
    private int _currentPage = 1;
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadContactData();
    }
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private async Task LoadContactData()
    {
        IsLoading = true;
        try
        {
            // Load contact details
            Result<ContactDetailsDto>? result = await _apiService.GetContactDetails(ContactId);
            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                _contact = result.Value;
            }
            else
            {
                _snackbar?.Add("Error loading contact details", Severity.Error);
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
            Result<PagedResponse<ListJobDto>>? jobsResult = await _apiService.GetContactJobs(ContactId, page, _rowsPerPage, SortDirectionEnum.Desc, null);
            if (jobsResult is not null && jobsResult.IsSuccess && jobsResult.Value is not null)
            {
                _pagedJobs = jobsResult.Value;
                _currentPage = page;
            }
            else
            {
                _snackbar?.Add("Error loading contact jobs", Severity.Error);
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
        if (_contact?.address is null)
            return "No address";

        List<string> parts = [];
        if (!string.IsNullOrWhiteSpace(_contact.address.suburb))
            parts.Add(_contact.address.suburb.ToUpper());
        if (!string.IsNullOrWhiteSpace(_contact.address.State?.ToString()))
            parts.Add(_contact.address.State.ToString());
        if (!string.IsNullOrWhiteSpace(_contact.address.postCode))
            parts.Add(_contact.address.postCode);

        return parts.Count > 0 ? string.Join(" ", parts) : "No address";
    }
}

