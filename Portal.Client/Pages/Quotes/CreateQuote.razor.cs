using MudBlazor;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Quote;
using Portal.Shared.DTO.Types;
using Portal.Shared.ResponseModels;
using System.Globalization;

namespace Portal.Client.Pages.Quotes;

public partial class CreateQuote
{
    private static readonly CultureInfo QuoteCurrencyCulture = new("en-US");

    private QuoteTemplateDto[] _templates = [];
    private QuoteCreationDto _model = new() { Address = new() { State = Portal.Shared.StateEnum.VIC, StateId = (int)Portal.Shared.StateEnum.VIC }, QuoteStatusId = (int)QuoteStatusEnum.Draft, QuoteTypeId = (int)JobTypeEnum.Surveying };
    private int _stepperIndex = 0;
    private ListContactDto? _jobContact;
    private ServiceTypeDto? _serviceType;
    private decimal _price;
    private string _description = string.Empty;
    private ServiceTypeDto[] _services = [];
    private bool _submitting;

    protected override async Task OnInitializedAsync()
    {
        Result<QuoteTemplateDto[]>? templatesResult = await _apiService.GetQuotingTemplates();
        if (templatesResult?.IsSuccess == true && templatesResult.Value is not null)
            _templates = templatesResult.Value;

        Result<AllSettingsTypesDto> servicesResult = await _apiService.GetAllSettingsTypes();
        if (servicesResult?.IsSuccess == true && servicesResult.Value is not null)
            _services = servicesResult.Value.ServiceTypes;

    }

    private decimal QuoteItemsTotal => _model.QuoteItems.Sum(i => i.Price);

    private async Task SaveQuote(QuoteStatusEnum status)
    {
        if (_model.ContactId <= 0 || _jobContact is null)
        {
            _snackbar.Add("Please select a contact.", Severity.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_model.Address.Street) && string.IsNullOrWhiteSpace(_model.Address.Suburb))
        {
            _snackbar.Add("Please enter an address (at least street or suburb).", Severity.Warning);
            return;
        }

        _model.QuoteStatusId = (int)status;
        _submitting = true;
        try
        {
            Result<int> res = await _apiService.CreateQuote(_model);
            if (res is null || !res.IsSuccess)
            {
                _snackbar.Add(res?.ErrorDescription ?? "Failed to create quote. Please try again.", Severity.Error);
                return;
            }

            _snackbar.Add("Quote created.", Severity.Success);
            _navigationManager.NavigateTo($"/quotes/{res.Value}");
        }
        finally
        {
            _submitting = false;
        }
    }

    /// <summary>
    /// Used by the type ahead auto complete for searching contacts
    /// </summary>
    /// <param name="search">The search string</param>
    /// <param name="token">The token</param>
    /// <returns></returns>
    private async Task<IEnumerable<ListContactDto>> SearchContacts(string search, CancellationToken token)
    {
        ContactFilterDto filter = new(Page: 1, PageSize: 100, SearchFilter: search, OrderBy: $"{nameof(ListContactDto.FullName)}", Order: Portal.Shared.SortDirectionEnum.Desc);
        Result<PagedResponse<ListContactDto>>? contactResult = await _apiService.GetAllContacts(filter);

        if (contactResult?.IsSuccess == true && contactResult.Value?.Result is not null)
            return contactResult.Value.Result;
        else
            return [];
    }

    /// <summary>
    /// Handles the selected contact change from the type-ahead and updates the job creation model.
    /// </summary>
    /// <param name="value">The selected contact, or null if cleared.</param>
    private void OnContactChanged(ListContactDto value)
    {
        _jobContact = value;
        _model.ContactId = value.ContactId;
    }

    private void AddService()
    {
        if (_serviceType is not null)
            _model.QuoteItems.Add(new QuoteItemDto { ServiceName = _serviceType.ServiceName, ServiceTypeId = _serviceType.Id, Description = _serviceType?.Description ?? string.Empty, Price = _price });
    }

    /// <summary>
    /// Removes a service from the quote creation model's list of quote items. This is called when the user clicks the remove button for a service in the UI.
    /// </summary>
    /// <param name="service"></param>
    private void RemoveService(QuoteItemDto service)
    {
        _model.QuoteItems.Remove(service);
    }

    /// <summary>
    /// Handles the selection of a quote template. It updates the quote creation model with the details from the selected template and moves to the next step in the stepper.
    /// </summary>
    /// <param name="template"></param>
    private void SelectTemplate(QuoteTemplateDto template)
    {
        _model.Description = template.Description;
        _model.QuoteItems = [.. template.QuoteTemplateItems.Select(qi => new QuoteItemDto {
            Description = qi.Description,
            ServiceName = qi.ServiceName,
            ServiceTypeId = qi.ServiceTypeId,
            Price = qi.DefaultPrice })];
    }
}