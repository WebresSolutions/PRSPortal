using MudBlazor;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Quote;
using Portal.Shared.DTO.Types;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Quotes;

public partial class CreateQuote
{
    private QuoteTemplateDto[] _templates = [];
    private QuoteCreationDto _model = new() { Address = new() };
    private int _stepperIndex = 0;
    private ListContactDto? _jobContact;
    private ServiceTypeDto? _serviceType;
    private decimal _price;
    private string _description = string.Empty;
    private MudStep _mudStep = new();
    private ServiceTypeDto[] _services = [];

    protected override async Task OnInitializedAsync()
    {
        Result<QuoteTemplateDto[]>? templatesResult = await _apiService.GetQuotingTemplates();
        if (templatesResult?.IsSuccess == true && templatesResult.Value is not null)
            _templates = templatesResult.Value;

        Result<AllSettingsTypesDto> servicesResult = await _apiService.GetAllSettingsTypes();
        if (servicesResult?.IsSuccess == true && servicesResult.Value is not null)
            _services = servicesResult.Value.ServiceTypes;

    }

    private async Task SaveQuote()
    {
        Result<int> res = await _apiService.CreateQuote(_model);
        if (res?.IsSuccess != true)
            _snackbar.Add("Failed to create quote. Please try again.", Severity.Error);

        _navigationManager.NavigateTo($"/quotes/{res!.Value}");
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
        _model.QuoteTypeId = template.QuoteTemplateItems.FirstOrDefault()?.ServiceTypeId ?? 0;
        _model.QuoteItems = [.. template.QuoteTemplateItems.Select(qi => new QuoteItemDto {
            Description = qi.Description,
            ServiceName = qi.ServiceName,
            ServiceTypeId = qi.ServiceTypeId,
            Price = qi.DefaultPrice })];
        _stepperIndex = 1;
    }
}