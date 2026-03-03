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

    #region Private Fields
    private ContactDetailsDto? _contact;
    private PagedResponse<ListJobDto>? _pagedJobs;
    private readonly int _rowsPerPage = 15;
    private int _currentPage = 1;
    #endregion

    /// <summary>
    /// When parameters are set or changed, loads contact data and jobs for the current contact.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadContactData();
    }

    /// <summary>
    /// Initializes the component; contact data is loaded in OnParametersSetAsync when ContactId is available.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Loads the contact details and first page of jobs from the API for the current contact.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
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

    /// <summary>
    /// Loads a page of jobs associated with the current contact from the API.
    /// </summary>
    /// <param name="page">The page number to load.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadJobs(int page)
    {
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
        }
    }

    /// <summary>
    /// Returns a formatted address string (suburb, state, post code) for the current contact.
    /// </summary>
    /// <returns>The formatted address or "No address" if none is set.</returns>
    private string GetAddressString() => _contact?.Address?.ToDisplayString() ?? "No address";
}

