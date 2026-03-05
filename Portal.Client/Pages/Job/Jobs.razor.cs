using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;
using Portal.Shared.Web;

namespace Portal.Client.Pages.Job;

public partial class Jobs : IDisposable
{
    #region Query Parameters
    [Parameter]
    [SupplyParameterFromQuery(Name = "page")]
    public int? Page { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "pageSize")]
    public int? PageSize { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "address")]
    public string? AddressSearch { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "jobNumber")]
    public string? JobNumberSearch { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "contact")]
    public string? ContactSearch { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "order")]
    public string? Order { get; set; }
    #endregion

    #region Private Fields 
    /// <summary>
    /// The paged response containing the current page of jobs and total count, used to bind data to the grid. Updated on each data load from the API.
    /// </summary>
    private PagedResponse<ListJobDto>? _pagedResponse;
    /// <summary>
    /// Rows per page for the grid, defaulting to 25. This is used as the default page size if not specified in query parameters and can be updated by the user through the grid's pagination controls, which will then update the URL query parameters accordingly.
    /// </summary>
    private readonly int _rowsPerPage = 25;
    /// <summary>
    /// The reference to the MudDataGrid component, used to trigger data reloads when filter criteria change. 
    /// This allows external methods (e.g., search input handlers) to refresh the grid data by calling _grid.ReloadServerData() after updating the filter state and URL query parameters. The grid will call the LoadJobs method to fetch the updated data from the API based on the current filter state.
    /// </summary>
    private MudDataGrid<ListJobDto>? _grid;
    /// <summary>
    /// Current filter and pagination state, synced from and to query parameters.
    /// </summary>
    private SessionSearchData _filterState = new();
    #endregion

    /// <summary>
    /// Called when the component is initialized. Initializes filter state from query parameters.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        SyncStateFromQueryParameters();
    }

    /// <summary>
    /// Called when component parameters are set or changed. Syncs state from query parameters and reloads grid if needed.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        SyncStateFromQueryParameters();
        if (_grid is not null)
            await _grid.ReloadServerData();
    }

    /// <summary>
    /// Syncs filter state from the current query parameters.
    /// </summary>
    private void SyncStateFromQueryParameters()
    {
        if (Page.HasValue)
            _filterState.Page = Page.Value;
        if (PageSize.HasValue)
            _filterState.PageSize = PageSize.Value;
        if (AddressSearch is not null)
            _filterState.AddressSearch = AddressSearch;
        if (ContactSearch is not null)
            _filterState.ContactSearch = ContactSearch;
        if (JobNumberSearch is not null)
            _filterState.JobNumberSearch = JobNumberSearch;
        if (!string.IsNullOrWhiteSpace(Order) && Enum.TryParse<SortDirectionEnum>(Order, true, out SortDirectionEnum orderEnum))
            _filterState.Order = orderEnum;
    }

    /// <summary>
    /// Updates the URL query parameters to match the current filter state.
    /// </summary>
    private void UpdateUrlFromState()
    {
        Dictionary<string, string?> queryParams = [];

        if (_filterState.Page > 0)
            queryParams["page"] = _filterState.Page.ToString();

        if (_filterState.PageSize != 25)
            queryParams["pageSize"] = _filterState.PageSize.ToString();

        if (!string.IsNullOrWhiteSpace(_filterState.AddressSearch))
            queryParams["address"] = _filterState.AddressSearch;

        if (!string.IsNullOrWhiteSpace(_filterState.ContactSearch))
            queryParams["contact"] = _filterState.ContactSearch;

        if (!string.IsNullOrWhiteSpace(_filterState.JobNumberSearch))
            queryParams["jobNumber"] = _filterState.JobNumberSearch;

        if (_filterState.Order != SortDirectionEnum.Asc)
            queryParams["order"] = _filterState.Order.ToString();

        string queryString = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value ?? "")}"));
        string basePath = _navigationManager.ToBaseRelativePath(_navigationManager.Uri).Split('?')[0];
        string newUrl = queryString.Length > 0 ? $"{basePath}?{queryString}" : basePath;
        string currentRelativePath = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
        string currentPathOnly = currentRelativePath.Split('?')[0];

        if (newUrl != currentRelativePath && newUrl != currentPathOnly)
            _navigationManager.NavigateTo(newUrl, replace: true);
    }

    /// <summary>
    /// This method is called by the MudDataGrid to fetch data when needed (paging, sorting, filtering).
    /// Implements server-side data loading with pagination and search capabilities.
    /// </summary>
    /// <param name="state">The current grid state containing pagination and sorting information</param>
    /// <returns>A GridData object containing the current page of facilities and total count</returns>
    private async Task<GridData<ListJobDto>> LoadJobs(GridState<ListJobDto> state)
    {
        IsLoading = true;

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
            UpdateUrlFromState();
            JobFilterDto search = new(apiPageNumber, apiPageSize, _filterState.AddressSearch, _filterState.ContactSearch, _filterState.JobNumberSearch, _filterState.OrderBy, _filterState.Order, _filterState.ShowDeleted, null, null);
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
                _snackbar?.Add("Error", Severity.Error);

                return new GridData<ListJobDto>() { Items = [], TotalItems = 0 };
            }
        }
        catch (Exception)
        {
            _snackbar?.Add("Error Loading Jobs", Severity.Error);
            return new GridData<ListJobDto>() { Items = [], TotalItems = 0 };
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Manually refreshes the grid's data from another action (e.g., after adding/editing a facility)
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task RefreshGridData()
    {
        if (_grid is not null)
            await _grid.ReloadServerData();
    }

    /// <summary>
    /// Handles search input changes and triggers grid data reload with the new search criteria
    /// </summary>
    /// <param name="text">The search text entered by the user</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private Task OnSearch()
    {
        _filterState.Page = 0;
        UpdateUrlFromState();
        if (_grid is not null)
            return _grid!.ReloadServerData();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Toggles visibility of deleted jobs based on the selected tab and reloads the grid.
    /// </summary>
    /// <param name="tabName">The name of the selected tab (e.g. "Deleted").</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ShowDelete(TabTypeEnum tabType)
    {
        _filterState.ShowDeleted = tabType is TabTypeEnum.Deleted;
        if (_grid is not null)
            await _grid.ReloadServerData();
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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _grid?.Dispose();
    }
}