using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Quote;
using Portal.Shared.ResponseModels;
using Portal.Shared.Web;

namespace Portal.Client.Pages.Quotes;

public partial class Quotes : IDisposable
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
    private PagedResponse<QuoteListDto>? _pagedResponse;
    private readonly int _rowsPerPage = 25;
    private MudDataGrid<QuoteListDto>? _grid;
    private SessionSearchData _filterState = new();
    #endregion

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        SyncStateFromQueryParameters();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        SyncStateFromQueryParameters();
        if (_grid is not null)
            await _grid.ReloadServerData();
    }

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
        if (!string.IsNullOrWhiteSpace(Order) && Enum.TryParse(Order, true, out SortDirectionEnum orderEnum))
            _filterState.Order = orderEnum;
    }

    private void UpdateUrlFromState()
    {
        Dictionary<string, string?> queryParams = [];

        if (_filterState.Page > 0)
            queryParams["page"] = _filterState.Page.ToString();

        if (_filterState.PageSize != 25)
            queryParams["pageSize"] = _filterState.PageSize.ToString();

        if (!string.IsNullOrWhiteSpace(AddressSearch))
            queryParams["address"] = AddressSearch;

        if (!string.IsNullOrWhiteSpace(ContactSearch))
            queryParams["contact"] = ContactSearch;

        if (!string.IsNullOrWhiteSpace(JobNumberSearch))
            queryParams["jobNumber"] = JobNumberSearch;

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

    private static string MapSortToApiOrderBy(string? sortBy)
    {
        if (string.IsNullOrEmpty(sortBy))
            return nameof(QuoteListDto.Id);

        if (sortBy.Equals(nameof(QuoteListDto.Id), StringComparison.OrdinalIgnoreCase))
            return nameof(QuoteListDto.Id);

        if (sortBy.Equals(nameof(QuoteListDto.QuoteReference), StringComparison.OrdinalIgnoreCase))
            return nameof(QuoteListDto.Id);

        if (sortBy.Equals(nameof(QuoteListDto.TotalPrice), StringComparison.OrdinalIgnoreCase))
            return nameof(QuoteListDto.Id);

        // API matches Contact name as nameof(QuoteListDto.Contact.fullName) => "fullName"
        if (sortBy.Equals("fullName", StringComparison.OrdinalIgnoreCase)
            || sortBy.Equals("Contact.fullName", StringComparison.OrdinalIgnoreCase)
            || sortBy.Equals($"{nameof(QuoteListDto.Contact)}.{nameof(ContactDto.fullName)}", StringComparison.OrdinalIgnoreCase))
            return nameof(QuoteListDto.Contact.fullName);

        string street = $"{nameof(QuoteListDto.Address)}.{nameof(AddressDto.Street)}";
        string suburb = $"{nameof(QuoteListDto.Address)}.{nameof(AddressDto.Suburb)}";
        string postCode = $"{nameof(QuoteListDto.Address)}.{nameof(AddressDto.PostCode)}";

        if (sortBy.Equals(street, StringComparison.OrdinalIgnoreCase)
            || sortBy.Equals(nameof(AddressDto.Street), StringComparison.OrdinalIgnoreCase))
            return street;

        if (sortBy.Equals(suburb, StringComparison.OrdinalIgnoreCase)
            || sortBy.Equals(nameof(AddressDto.Suburb), StringComparison.OrdinalIgnoreCase))
            return suburb;

        if (sortBy.Equals(postCode, StringComparison.OrdinalIgnoreCase)
            || sortBy.Equals(nameof(AddressDto.PostCode), StringComparison.OrdinalIgnoreCase))
            return postCode;

        if (sortBy.Contains(nameof(QuoteListDto.Address), StringComparison.OrdinalIgnoreCase))
        {
            if (sortBy.Contains(nameof(AddressDto.Street), StringComparison.OrdinalIgnoreCase))
                return street;
            if (sortBy.Contains(nameof(AddressDto.Suburb), StringComparison.OrdinalIgnoreCase))
                return suburb;
            if (sortBy.Contains(nameof(AddressDto.PostCode), StringComparison.OrdinalIgnoreCase))
                return postCode;
        }

        return nameof(QuoteListDto.Id);
    }

    private async Task<GridData<QuoteListDto>> LoadQuotes(GridState<QuoteListDto> state)
    {
        IsLoading = true;

        try
        {
            int apiPageNumber = state.Page;
            int apiPageSize = state.PageSize;
            apiPageNumber++;

            SortDefinition<QuoteListDto>? sortDefinition = state.SortDefinitions.FirstOrDefault();
            if (sortDefinition is not null)
            {
                _filterState.Order = sortDefinition.Descending ? SortDirectionEnum.Desc : SortDirectionEnum.Asc;
                _filterState.OrderBy = MapSortToApiOrderBy(sortDefinition.SortBy);
            }
            else
            {
                _filterState.OrderBy = nameof(QuoteListDto.Id);
                _filterState.Order = SortDirectionEnum.Desc;
            }

            _filterState.Page = state.Page;
            _filterState.PageSize = state.PageSize;
            UpdateUrlFromState();

            QuoteFilterDto search = new(
                apiPageNumber,
                apiPageSize,
                JobNumberSearch,
                ContactSearch,
                AddressSearch,
                _filterState.OrderBy,
                _filterState.Order,
                _filterState.ShowDeleted);

            Result<PagedResponse<QuoteListDto>>? apiResult = await _apiService.GetAllQuotes(search);

            if (apiResult is not null && apiResult.IsSuccess && apiResult.Value is not null)
            {
                _pagedResponse = apiResult.Value;
                return new GridData<QuoteListDto>()
                {
                    Items = _pagedResponse.Result ?? Enumerable.Empty<QuoteListDto>(),
                    TotalItems = _pagedResponse.TotalCount
                };
            }

            _snackbar?.Add("Error loading quotes", Severity.Error);
            return new GridData<QuoteListDto>() { Items = [], TotalItems = 0 };
        }
        catch (Exception)
        {
            _snackbar?.Add("Error loading quotes", Severity.Error);
            return new GridData<QuoteListDto>() { Items = [], TotalItems = 0 };
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task RefreshGridData()
    {
        if (_grid is not null)
            await _grid.ReloadServerData();
    }

    private Task OnSearch()
    {
        _filterState.Page = 0;
        UpdateUrlFromState();
        if (_grid is not null)
            return _grid.ReloadServerData();

        return Task.CompletedTask;
    }

    private async Task ShowDelete(TabTypeEnum tabType)
    {
        _filterState.ShowDeleted = tabType is TabTypeEnum.Deleted;
        if (_grid is not null)
            await _grid.ReloadServerData();
    }

    private async Task RemoveQuote(int quoteId)
    {
        bool? confirm = await _dialog.ShowMessageBox(
            "Confirm Delete",
            "Are you sure you want to delete this quote?",
            yesText: "Delete",
            cancelText: "Cancel",
            options: new DialogOptions { CloseOnEscapeKey = true });
        if (confirm == true)
        {
            Result<bool> result = await _apiService.DeleteQuote(quoteId);
            if (result.IsSuccess)
            {
                _snackbar.Add("Quote deleted.", Severity.Success);
                await RefreshGridData();
            }
            else
                _snackbar.Add(result.ErrorDescription ?? "Failed to delete quote.", Severity.Error);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _grid?.Dispose();
    }
}
