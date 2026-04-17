using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Quote;
using Portal.Shared.DTO.Types;
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
    private MudDataGrid<QuoteListDto> _grid;
    private SessionSearchData _filterState = new();
    private QuoteTemplateDto[] _quotingTemplates = [];
    private ServiceTypeDto[] _serviceTypesForTemplates = [];
    private string _newTemplateName = "";
    private string? _newTemplateDescription;
    private JobTypeEnum _newTemplateJobType = JobTypeEnum.Construction;
    private readonly List<NewTemplateLineEditor> _newTemplateLines = [];
    private bool _templateFormBusy;
    private QuoteTemplateDto? _editingTemplateSnapshot;
    #endregion

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        SyncStateFromQueryParameters();
        await LoadQuotingTemplateSectionAsync();
        _breadCrumbService.SetBreadCrumbItems(
          [
            new("Quotes", href: "/quotes", disabled: true)
          ]);
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

    private async Task<GridData<QuoteListDto>> LoadQuotes(GridState<QuoteListDto> state, CancellationToken? cancellationToken = null)
    {
        IsLoading = true;

        try
        {
            cancellationToken?.ThrowIfCancellationRequested();
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

    private async Task LoadQuotingTemplateSectionAsync()
    {
        Task<Result<QuoteTemplateDto[]>> templatesTask = _apiService.GetQuotingTemplates();
        Task<Result<AllSettingsTypesDto>> typesTask = _apiService.GetAllSettingsTypes();
        await Task.WhenAll(templatesTask, typesTask);

        Result<QuoteTemplateDto[]> templatesResult = await templatesTask;
        if (templatesResult.IsSuccess && templatesResult.Value is not null)
            _quotingTemplates = templatesResult.Value;

        Result<AllSettingsTypesDto> typesResult = await typesTask;
        if (typesResult.IsSuccess && typesResult.Value?.ServiceTypes is { Length: > 0 } st)
            _serviceTypesForTemplates = [.. st.Where(s => s.IsActive != false)];

        if (!templatesResult.IsSuccess)
            _snackbar?.Add(templatesResult.ErrorDescription ?? "Could not load quoting templates.", Severity.Warning);
        if (!typesResult.IsSuccess)
            _snackbar?.Add(typesResult.ErrorDescription ?? "Could not load service types for templates.", Severity.Warning);
    }

    private void AddNewTemplateLine()
    {
        int? defaultServiceId = _serviceTypesForTemplates.FirstOrDefault()?.Id;
        _newTemplateLines.Add(new NewTemplateLineEditor { ServiceTypeId = defaultServiceId, TemplateItemId = 0 });
        StateHasChanged();
    }

    private void BeginEditTemplate(QuoteTemplateDto template)
    {
        _editingTemplateSnapshot = template;
        _newTemplateName = template.Name;
        _newTemplateDescription = template.Description;
        _newTemplateJobType = template.JobType;
        _newTemplateLines.Clear();
        foreach (QuoteTemplateItemDto item in template.QuoteTemplateItems ?? [])
        {
            _newTemplateLines.Add(new NewTemplateLineEditor
            {
                TemplateItemId = item.Id,
                ServiceTypeId = item.ServiceTypeId,
                DefaultPrice = item.DefaultPrice,
                Description = item.Description
            });
        }

        StateHasChanged();
    }

    private void CancelTemplateEdit()
    {
        _editingTemplateSnapshot = null;
        _newTemplateName = "";
        _newTemplateDescription = null;
        _newTemplateJobType = JobTypeEnum.Construction;
        _newTemplateLines.Clear();
        StateHasChanged();
    }

    private async Task DeleteQuotingTemplateAsync(QuoteTemplateDto template)
    {
        bool? confirm = await _dialog.ShowMessageBoxAsync(
            "Delete quoting template",
            $"Delete template \"{template.Name}\"? This cannot be undone.",
            yesText: "Delete",
            cancelText: "Cancel",
            options: new DialogOptions { CloseOnEscapeKey = true });
        if (confirm != true)
            return;

        Result<bool> result = await _apiService.DeleteQuotingTemplate(template.Id);
        if (result.IsSuccess)
        {
            _snackbar?.Add("Template deleted.", Severity.Success);
            if (_editingTemplateSnapshot?.Id == template.Id)
                CancelTemplateEdit();
            await LoadQuotingTemplateSectionAsync();
        }
        else
            _snackbar?.Add(result.ErrorDescription ?? "Failed to delete template.", Severity.Error);
    }

    private void RemoveNewTemplateLine(NewTemplateLineEditor line)
    {
        _newTemplateLines.Remove(line);
        StateHasChanged();
    }

    private async Task SaveQuotingTemplateAsync()
    {
        string name = _newTemplateName.Trim();
        if (string.IsNullOrEmpty(name))
        {
            _snackbar?.Add("Template name is required.", Severity.Warning);
            return;
        }

        List<NewTemplateLineEditor> linesWithService = _newTemplateLines.Where(l => l.ServiceTypeId.HasValue).ToList();
        if (linesWithService.Count != _newTemplateLines.Count)
        {
            _snackbar?.Add("Each line must have a service selected, or remove empty lines.", Severity.Warning);
            return;
        }

        QuoteTemplateItemDto[] items = [.. linesWithService.Select(l =>
        {
            ServiceTypeDto svc = _serviceTypesForTemplates.First(s => s.Id == l.ServiceTypeId!.Value);
            string? desc = string.IsNullOrWhiteSpace(l.Description) ? null : l.Description.Trim();
            return new QuoteTemplateItemDto(l.TemplateItemId, svc.Id, svc.ServiceName, desc, l.DefaultPrice);
        })];

        string? description = string.IsNullOrWhiteSpace(_newTemplateDescription) ? null : _newTemplateDescription.Trim();

        _templateFormBusy = true;
        try
        {
            if (_editingTemplateSnapshot is { } snapshot)
            {
                QuoteTemplateDto payload = snapshot with
                {
                    Name = name,
                    Description = description,
                    JobType = _newTemplateJobType,
                    QuoteTemplateItems = items
                };

                Result<QuoteTemplateDto> result = await _apiService.UpdateQuotingTemplate(payload);
                if (result.IsSuccess)
                {
                    _snackbar?.Add("Template updated.", Severity.Success);
                    CancelTemplateEdit();
                    await LoadQuotingTemplateSectionAsync();
                }
                else
                    _snackbar?.Add(result.ErrorDescription ?? "Failed to update quoting template.", Severity.Error);
            }
            else
            {
                QuoteTemplateDto payload = new(
                    0,
                    name,
                    description,
                    true,
                    default,
                    null,
                    null,
                    _newTemplateJobType,
                    items);

                Result<QuoteTemplateDto> result = await _apiService.CreateQuotingTemplate(payload);
                if (result.IsSuccess)
                {
                    _snackbar?.Add("Quoting template created.", Severity.Success);
                    _newTemplateName = "";
                    _newTemplateDescription = null;
                    _newTemplateJobType = JobTypeEnum.Construction;
                    _newTemplateLines.Clear();
                    await LoadQuotingTemplateSectionAsync();
                }
                else
                    _snackbar?.Add(result.ErrorDescription ?? "Failed to create quoting template.", Severity.Error);
            }
        }
        finally
        {
            _templateFormBusy = false;
        }
    }

    private sealed class NewTemplateLineEditor
    {
        public int TemplateItemId { get; set; }
        public int? ServiceTypeId { get; set; }
        public decimal DefaultPrice { get; set; }
        public string? Description { get; set; }
    }

    private async Task RemoveQuote(int quoteId)
    {
        bool? confirm = await _dialog.ShowMessageBoxAsync(
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
