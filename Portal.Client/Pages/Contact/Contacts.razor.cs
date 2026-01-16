using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared;
using Portal.Shared.DTO.Contact;
using Portal.Shared.ResponseModels;
using Portal.Shared.Web;

namespace Portal.Client.Pages.Contact;

public partial class Contacts
{
    private PagedResponse<ListContactDto>? _pagedResponse;
    private readonly int _rowsPerPage = 25;
    private MudDataGrid<ListContactDto>? _grid;
    /// <summary>
    /// Session data for the contacts page
    /// </summary>
    private SessionSearchData _sessionData = new();

    #region Constants
    private const string _ContactsSessionKey = "ContactsListSession";
    #endregion

    #region Query Parameters
    [Parameter]
    [SupplyParameterFromQuery]
    public int? Page { get; set; }

    [Parameter]
    [SupplyParameterFromQuery]
    public int? PageSize { get; set; }

    [Parameter]
    [SupplyParameterFromQuery]
    public string? Search { get; set; }

    [Parameter]
    [SupplyParameterFromQuery]
    public string? Order { get; set; }
    #endregion

    /// <summary>
    /// Called when the component is initialized.
    /// Data loading for the grid is now handled by LoadContactsServerData.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Read from query parameters first, then fall back to session storage
        if (Page.HasValue)
            _sessionData.Page = Page.Value;
        if (PageSize.HasValue)
            _sessionData.PageSize = PageSize.Value;
        if (!string.IsNullOrWhiteSpace(Search))
            _sessionData.SearchString = Search;
        if (!string.IsNullOrWhiteSpace(Order) && Enum.TryParse<SortDirectionEnum>(Order, true, out SortDirectionEnum orderEnum))
            _sessionData.Order = orderEnum;

        // Restore session data only if query parameters weren't provided
        if (!Page.HasValue && !PageSize.HasValue && string.IsNullOrWhiteSpace(Search) && string.IsNullOrWhiteSpace(Order))
        {
            SessionSearchData? savedSession = _sessionStorage.GetItem<SessionSearchData>(_ContactsSessionKey);
            if (savedSession is not null)
                _sessionData = savedSession;
        }

        // Update URL if query parameters don't match session data
        UpdateUrlFromSessionData();
    }

    /// <summary>
    /// Called when component parameters are set or changed
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        // Update session data from query parameters if they changed
        bool dataChanged = false;
        if (Page.HasValue && _sessionData.Page != Page.Value)
        {
            _sessionData.Page = Page.Value;
            dataChanged = true;
        }
        if (PageSize.HasValue && _sessionData.PageSize != PageSize.Value)
        {
            _sessionData.PageSize = PageSize.Value;
            dataChanged = true;
        }
        if (!string.IsNullOrWhiteSpace(Search) && _sessionData.SearchString != Search)
        {
            _sessionData.SearchString = Search;
            dataChanged = true;
        }
        if (!string.IsNullOrWhiteSpace(Order) && Enum.TryParse<SortDirectionEnum>(Order, true, out SortDirectionEnum orderEnum) && _sessionData.Order != orderEnum)
        {
            _sessionData.Order = orderEnum;
            dataChanged = true;
        }

        if (dataChanged)
        {
            SaveSessionData();
            if (_grid is not null)
                await _grid.ReloadServerData();
        }
    }

    /// <summary>
    /// Saves the current session data to session storage and updates URL
    /// </summary>
    private void SaveSessionData()
    {
        _sessionStorage.SetItem(_ContactsSessionKey, _sessionData);
        UpdateUrlFromSessionData();
    }

    /// <summary>
    /// Updates the URL query parameters based on the current session data
    /// </summary>
    private void UpdateUrlFromSessionData()
    {
        Dictionary<string, string?> queryParams = new();

        // Build query parameters
        if (_sessionData.Page > 0)
            queryParams["page"] = _sessionData.Page.ToString();

        if (_sessionData.PageSize != 25) // Only add if not default
            queryParams["pageSize"] = _sessionData.PageSize.ToString();

        if (!string.IsNullOrWhiteSpace(_sessionData.SearchString))
            queryParams["search"] = _sessionData.SearchString;

        if (_sessionData.Order != SortDirectionEnum.Asc) // Only add if not default
            queryParams["order"] = _sessionData.Order.ToString();

        // Build query string
        var queryString = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value ?? "")}"));
        string basePath = _navigationManager.ToBaseRelativePath(_navigationManager.Uri).Split('?')[0];
        string newUrl = queryString.Length > 0 ? $"{basePath}?{queryString}" : basePath;

        // Get current URL without query string for comparison
        string currentRelativePath = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
        string currentPathOnly = currentRelativePath.Split('?')[0];

        // Only navigate if URL actually changed to avoid unnecessary navigation
        if (newUrl != currentRelativePath && newUrl != currentPathOnly)
        {
            _navigationManager.NavigateTo(newUrl, replace: true);
        }
    }

    /// <summary>
    /// This method is called by the MudDataGrid to fetch data when needed (paging, sorting, filtering).
    /// Implements server-side data loading with pagination and search capabilities.
    /// </summary>
    /// <param name="state">The current grid state containing pagination and sorting information</param>
    /// <returns>A GridData object containing the current page of contacts and total count</returns>
    private async Task<GridData<ListContactDto>> LoadContacts(GridState<ListContactDto> state)
    {
        IsLoading = true;

        try
        {
            int apiPageNumber = state.Page;
            int apiPageSize = state.PageSize;
            apiPageNumber++;

            // Handle sorting from grid state
            SortDefinition<ListContactDto>? sortDefinition = state.SortDefinitions.FirstOrDefault();
            if (sortDefinition != null)
            {
                // Set order direction
                _sessionData.Order = sortDefinition.Descending ? SortDirectionEnum.Desc : SortDirectionEnum.Asc;
                _sessionData.OrderBy = sortDefinition.SortBy switch
                {
                    string s when s == nameof(ListContactDto.FullName)
                            || s == nameof(ListContactDto.Email)
                            || s == nameof(ListContactDto.Phone)
                            || s == nameof(ListContactDto.Address) + "." + nameof(ListContactDto.Address.suburb)
                            || s == nameof(ListContactDto.Address) + "." + nameof(ListContactDto.Address.street)
                            || s == nameof(ListContactDto.Address) + "." + nameof(ListContactDto.Address.postCode) => s,
                    _ => nameof(ListContactDto.ContactId)
                };
            }
            else
            {
                // Default sorting if no sort definition
                _sessionData.OrderBy = nameof(ListContactDto.ContactId);
                _sessionData.Order = SortDirectionEnum.Desc;
            }

            // Save current state to session
            _sessionData.Page = state.Page;
            _sessionData.PageSize = state.PageSize;
            SaveSessionData();

            Result<PagedResponse<ListContactDto>>? apiResult = await _apiService.GetAllContacts(
                apiPageSize,
                apiPageNumber,
                _sessionData.SearchString,
                _sessionData.OrderBy,
                _sessionData.Order);

            if (apiResult is not null && apiResult.IsSuccess && apiResult.Value is not null)
            {
                _pagedResponse = apiResult.Value;
                // MudDataGrid requires GridData with Items for the current page and TotalItems count
                return new GridData<ListContactDto>()
                {
                    Items = _pagedResponse.Result ?? Enumerable.Empty<ListContactDto>(),
                    TotalItems = _pagedResponse.TotalCount
                };
            }
            else
            {
                _snackbar?.Add("Error loading contacts", Severity.Error);

                return new GridData<ListContactDto>() { Items = [], TotalItems = 0 };
            }
        }
        catch (Exception)
        {

            return new GridData<ListContactDto>() { Items = Enumerable.Empty<ListContactDto>(), TotalItems = 0 };
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Manually refreshes the grid's data from another action (e.g., after adding/editing a contact)
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
    private Task OnSearch(string text)
    {
        _sessionData.SearchString = text;
        // Reset to first page when search changes
        _sessionData.Page = 0;
        SaveSessionData();
        if (_grid is not null)
            return _grid!.ReloadServerData();

        return Task.CompletedTask;
    }

    private Task ClearSearch()
    {
        _sessionData.SearchString = string.Empty;
        _sessionData.Page = 0;
        _sessionStorage.RemoveItem(_ContactsSessionKey);
        _sessionData = new SessionSearchData(); // Reset to defaults
        return _grid!.ReloadServerData();
    }

    /// <summary>
    /// Handles marker click navigation to contact page
    /// </summary>
    /// <param name="contactId">The contact ID to navigate to</param>
    private Task NavigateToContact(int contactId)
    {
        _navigationManager.NavigateTo($"/contacts/{contactId}");
        return Task.CompletedTask;
    }
}

