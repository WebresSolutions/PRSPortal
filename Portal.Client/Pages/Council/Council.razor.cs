using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared;
using Portal.Shared.DTO.Councils;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;
using Portal.Shared.Web;

namespace Portal.Client.Pages.Council;

public partial class Council : IDisposable
{
    [Parameter]
    public required int CouncilId { get; set; }

    private CouncilDetailsDto? _council;
    private MudDataGrid<ListJobDto>? _jobGrid;
    private SessionSearchData _filterState = new() { PageSize = 10 };
    private PagedResponse<ListJobDto>? _pagedResponse;

    /// <summary>
    /// Initializes the component and loads council details and the first page of jobs.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        IsLoading = true;

        await LoadCouncilData();
        _breadCrumbService.SetBreadCrumbItems(
          [
            new("Councils", href: "/councils", disabled: false),
            new($"{_council?.CouncilName}", href: $"/councils/{CouncilId}", disabled: true)
          ]);
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
   // <summary>
    /// This method is called by the MudDataGrid to fetch data when needed (paging, sorting, filtering).
    /// Implements server-side data loading with pagination and search capabilities.
    /// </summary>
    /// <param name="state">The current grid state containing pagination and sorting information</param>
    /// <returns>A GridData object containing the current page of facilities and total count</returns>
    private async Task<GridData<ListJobDto>> LoadJobs(GridState<ListJobDto> state)
    {
        try
        {
            int apiPageNumber = state.Page;
            int apiPageSize = state.PageSize;
            apiPageNumber++;

            // Handle sorting from grid state
            SortDefinition<ListJobDto>? sortDefinition = state.SortDefinitions.FirstOrDefault();
            if (sortDefinition != null)
            {
                _filterState.Order = sortDefinition.Descending ? SortDirectionEnum.Desc : SortDirectionEnum.Asc;
                _filterState.OrderBy = sortDefinition.SortBy switch
                {
                    string s when s == nameof(ListJobDto.Contact1)
                            || s == nameof(ListJobDto.Address) + "." + nameof(ListJobDto.Address.PostCode)
                            || s == nameof(ListJobDto.Address) + "." + nameof(ListJobDto.Address.Suburb)
                            || s == nameof(ListJobDto.Address) + "." + nameof(ListJobDto.Address.Street)
                            || s == nameof(ListJobDto.JobNumber) => s,
                    _ => nameof(ListJobDto.JobId)
                };
            }
            else
            {
                _filterState.OrderBy = nameof(ListJobDto.JobId);
                _filterState.Order = SortDirectionEnum.Desc;
            }

            _filterState.Page = state.Page;
            _filterState.PageSize = state.PageSize;
            JobFilterDto search = new(apiPageNumber, apiPageSize, _filterState.AddressSearch, _filterState.ContactSearch, _filterState.JobNumberSearch, _filterState.OrderBy, _filterState.Order, _filterState.ShowDeleted, null, CouncilId);
            Result<PagedResponse<ListJobDto>>? apiResult = await _apiService.GetAllJobs(search);

            if (apiResult is not null && apiResult.IsSuccess && apiResult.Value is not null)
            {
                _pagedResponse = apiResult.Value;
                // MudDataGrid requires GridData with Items for the current page and TotalItems count
                return new GridData<ListJobDto>()
                {
                    Items = _pagedResponse.Result ?? Enumerable.Empty<ListJobDto>(),
                    TotalItems = _pagedResponse.TotalCount
                };
            }
            else
            {
                _snackbar?.Add("Error Loading Contact Jobs", Severity.Error);
                return new GridData<ListJobDto>() { Items = [], TotalItems = 0 };
            }
        }
        catch (Exception)
        {
            _snackbar?.Add("Error Loading Contact Jobs", Severity.Error);
            return new GridData<ListJobDto>() { Items = [], TotalItems = 0 };
        }
    }

    /// <summary>
    /// Prompts for confirmation and deletes the specified job, then refreshes the grid on success.
    /// </summary>
    /// <param name="jobId">The ID of the job to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RemoveJob(int jobId)
    {
        bool? confirm = await _dialog.ShowMessageBox(
            "Confirm Delete",
            "Are you sure you want to delete this Job?",
            yesText: "Delete",
            cancelText: "Cancel",
            options: new DialogOptions { CloseOnEscapeKey = true });
        if (confirm == true)
        {
            Result<bool> result = await _apiService.DeleteJob(jobId);
            if (result.IsSuccess)
            {
                _snackbar.Add("Job deleted.", Severity.Success);
                await RefreshGridData();
            }
            else
                _snackbar.Add(result.ErrorDescription ?? "Failed to delete Job.", Severity.Error);
        }

    }

    /// <summary>
    /// Manually refreshes the grid's data from another action (e.g., after adding/editing a facility)
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task RefreshGridData()
    {
        if (_jobGrid is not null)
            await _jobGrid.ReloadServerData();
    }

    /// <summary>
    /// Changes the current tab view and updates the displayed data based on the specified tab type.
    /// </summary>
    /// <param name="tab">The tab type to switch to. Use <see cref="TabTypeEnum.Deleted"/> to display deleted items; otherwise, active
    /// items are shown.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ChangeTabs(TabTypeEnum tab)
    {
        if (tab is TabTypeEnum.Deleted)
        {
            _filterState.ShowDeleted = true;
            await RefreshGridData();
        }
        else
        {
            _filterState.ShowDeleted = false;
            await RefreshGridData();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _jobGrid?.Dispose();
    }
}