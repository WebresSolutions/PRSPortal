using MudBlazor;
using Portal.Shared.DTO.Councils;
using Portal.Shared.ResponseModels;
using Portal.Shared.Web;

namespace Portal.Client.Pages.Council;

public partial class Councils
{
    private CouncilPartialDto[]? _allCouncils;
    private readonly int _rowsPerPage = 25;
    private MudDataGrid<CouncilPartialDto>? _grid;
    private int _currentPage = 0;
    private string _searchString = string.Empty;

    #region Constants
    private const string _CouncilsSessionKey = "CouncilsListSession";
    #endregion

    /// <summary>
    /// Called when the component is initialized.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Restore session data
        SessionSearchData? savedSession = _sessionStorage.GetItem<SessionSearchData>(_CouncilsSessionKey);
        if (savedSession is not null)
        {
            _currentPage = savedSession.Page;
            _searchString = savedSession.SearchString ?? string.Empty;
        }
    }

    /// <summary>
    /// This method is called by the MudDataGrid to fetch data when needed (paging, sorting, filtering).
    /// Implements client-side data loading with search capabilities.
    /// </summary>
    /// <param name="state">The current grid state containing pagination and sorting information</param>
    /// <returns>A GridData object containing the current page of councils and total count</returns>
    private async Task<GridData<CouncilPartialDto>> LoadCouncils(GridState<CouncilPartialDto> state)
    {
        IsLoading = true;

        try
        {
            // Load all councils if not already loaded
            if (_allCouncils is null)
            {
                Result<CouncilPartialDto[]>? apiResult = await _apiService.GetCouncils();

                if (apiResult is not null && apiResult.IsSuccess && apiResult.Value is not null)
                {
                    _allCouncils = apiResult.Value;
                }
                else
                {
                    _snackbar?.Add("Error loading councils", Severity.Error);

                    return new GridData<CouncilPartialDto>() { Items = [], TotalItems = 0 };
                }
            }

            // Apply search filter
            IEnumerable<CouncilPartialDto> filteredCouncils = _allCouncils;
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                string searchLower = _searchString.ToLowerInvariant();
                filteredCouncils = _allCouncils.Where(c =>
                    (c.councilName?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (c.phone?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (c.email?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (c.website?.ToLowerInvariant().Contains(searchLower) ?? false)
                );
            }

            // Apply sorting
            SortDefinition<CouncilPartialDto>? sortDefinition = state.SortDefinitions.FirstOrDefault();
            if (sortDefinition != null)
            {
                filteredCouncils = sortDefinition.SortBy switch
                {
                    nameof(CouncilPartialDto.councilId) => sortDefinition.Descending
                        ? filteredCouncils.OrderByDescending(c => c.councilId)
                        : filteredCouncils.OrderBy(c => c.councilId),
                    nameof(CouncilPartialDto.councilName) => sortDefinition.Descending
                        ? filteredCouncils.OrderByDescending(c => c.councilName)
                        : filteredCouncils.OrderBy(c => c.councilName),
                    nameof(CouncilPartialDto.phone) => sortDefinition.Descending
                        ? filteredCouncils.OrderByDescending(c => c.phone)
                        : filteredCouncils.OrderBy(c => c.phone),
                    nameof(CouncilPartialDto.email) => sortDefinition.Descending
                        ? filteredCouncils.OrderByDescending(c => c.email)
                        : filteredCouncils.OrderBy(c => c.email),
                    nameof(CouncilPartialDto.website) => sortDefinition.Descending
                        ? filteredCouncils.OrderByDescending(c => c.website)
                        : filteredCouncils.OrderBy(c => c.website),
                    _ => filteredCouncils
                };
            }
            else
            {
                // Default sorting by council name
                filteredCouncils = filteredCouncils.OrderBy(c => c.councilName);
            }

            // Convert to list for pagination
            List<CouncilPartialDto> councilsList = filteredCouncils.ToList();
            int totalCount = councilsList.Count;

            // Apply pagination
            int skip = state.Page * state.PageSize;
            IEnumerable<CouncilPartialDto> pagedCouncils = councilsList.Skip(skip).Take(state.PageSize);

            // Update current page
            _currentPage = state.Page;
            SaveSessionData();

            return new GridData<CouncilPartialDto>()
            {
                Items = pagedCouncils,
                TotalItems = totalCount
            };
        }
        catch (Exception ex)
        {
            _snackbar?.Add($"Error: {ex.Message}", Severity.Error);

            return new GridData<CouncilPartialDto>() { Items = [], TotalItems = 0 };
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Saves the current session data to session storage
    /// </summary>
    private void SaveSessionData()
    {
        SessionSearchData sessionData = new()
        {
            Page = _currentPage,
            SearchString = _searchString
        };
        _sessionStorage.SetItem(_CouncilsSessionKey, sessionData);
    }

    /// <summary>
    /// Manually refreshes the grid's data from another action (e.g., after adding/editing a council)
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task RefreshGridData()
    {
        // Clear cached data to force reload
        _allCouncils = null;
        if (_grid is not null)
            await _grid.ReloadServerData();
    }

    /// <summary>
    /// Handles search input changes and triggers grid data reload with the new search criteria
    /// </summary>
    /// <param name="text">The search text entered by the user</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private Task OnSearch(string text)
    {
        _searchString = text;
        // Reset to first page when search changes
        _currentPage = 0;
        SaveSessionData();
        if (_grid is not null)
            return _grid!.ReloadServerData();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Clears the search and resets the grid
    /// </summary>
    private Task ClearSearch()
    {
        _searchString = string.Empty;
        _currentPage = 0;
        _sessionStorage.RemoveItem(_CouncilsSessionKey);
        SaveSessionData();
        if (_grid is not null)
            return _grid!.ReloadServerData();

        return Task.CompletedTask;
    }
}