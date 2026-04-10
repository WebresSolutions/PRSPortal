using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Quote;
using Portal.Shared.DTO.Types;
using Portal.Shared.ResponseModels;
using System.Globalization;

namespace Portal.Client.Pages.Quotes;

public partial class EditQuote
{
    internal static readonly CultureInfo QuoteCurrencyCulture = new("en-US");

    private static readonly QuoteStatusEnum[] EditableStatuses = [QuoteStatusEnum.Draft, QuoteStatusEnum.New, QuoteStatusEnum.Rejected];

    [Parameter]
    public int QuoteId { get; set; }

    private QuoteUpdateDto? _model;
    private QuoteDetailsDto? _details;
    private ListContactDto? _jobContact;
    private QuoteTemplateDto[] _templates = [];
    private ServiceTypeDto? _serviceType;
    private decimal _price;
    private string _description = string.Empty;
    private ServiceTypeDto[] _services = [];
    private bool _submitting;
    private int? _loadedForQuoteId;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadSupportingDataAsync();
        await LoadQuoteIfNeededAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadQuoteIfNeededAsync();
    }

    private async Task LoadSupportingDataAsync()
    {
        Result<QuoteTemplateDto[]>? templatesResult = await _apiService.GetQuotingTemplates();
        if (templatesResult?.IsSuccess == true && templatesResult.Value is not null)
            _templates = templatesResult.Value;

        Result<AllSettingsTypesDto> servicesResult = await _apiService.GetAllSettingsTypes();
        if (servicesResult?.IsSuccess == true && servicesResult.Value is not null)
            _services = servicesResult.Value.ServiceTypes;
    }

    private async Task LoadQuoteIfNeededAsync()
    {
        if (_loadedForQuoteId == QuoteId)
            return;

        _loadedForQuoteId = QuoteId;
        IsLoading = true;
        try
        {
            Result<QuoteDetailsDto> result = await _apiService.GetQuoteDetails(QuoteId);
            if (!result.IsSuccess || result.Value is null)
            {
                _details = null;
                _model = null;
                _jobContact = null;
                if (result.Error != ErrorType.NotFound)
                    _snackbar?.Add(result.ErrorDescription ?? "Could not load quote.", Severity.Error);
                return;
            }

            _details = result.Value;
            _model = MapToUpdateDto(_details);
            _jobContact = _details.Contact;
            SetBreadcrumb();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void SetBreadcrumb()
    {
        string label = _details?.QuoteReference ?? QuoteId.ToString();
        _breadCrumbService.SetBreadCrumbItems(
        [
            new("Quotes", href: "/quotes", disabled: false),
            new(label, href: $"/quotes/{QuoteId}", disabled: false),
            new("Edit", href: $"/quotes/edit/{QuoteId}", disabled: true)
        ]);
    }

    private static QuoteUpdateDto MapToUpdateDto(QuoteDetailsDto d)
    {
        AddressDto address = d.Address != null
            ? new AddressDto(d.Address.AddressId, d.Address.State, d.Address.StateId, d.Address.Suburb ?? "", d.Address.Street ?? "", d.Address.PostCode ?? "")
            {
                LatLng = d.Address.LatLng
            }
            : new AddressDto(0, Portal.Shared.StateEnum.VIC, (int)Portal.Shared.StateEnum.VIC, "", "", "");

        List<QuoteItemDto> items = d.QuoteItems.Select(qi => new QuoteItemDto
        {
            Id = qi.Id,
            ServiceTypeId = qi.ServiceTypeId,
            ServiceName = qi.ServiceName,
            Price = qi.Price,
            Description = qi.Description,
            IsEditing = false
        }).ToList();

        return new QuoteUpdateDto
        {
            QuoteId = d.Id,
            QuoteReferenceNumber = d.QuoteReference,
            Description = d.Description,
            QuoteStatusId = d.QuotesStatus.StatusEnum,
            JobType = (JobTypeEnum)d.QuoteTypeId,
            ContactId = d.Contact?.ContactId ?? 0,
            TargetDeliveryDate = d.TargetDeliveryDate,
            Address = address,
            QuoteItems = items
        };
    }

    private bool CanEdit => _details is not null && EditableStatuses.Contains(_details.QuotesStatus.StatusEnum);

    private decimal QuoteItemsTotal => _model?.QuoteItems.Sum(i => i.Price) ?? 0;

    private async Task SaveAsync()
    {
        if (_model is null)
            return;
        if (!CanEdit)
        {
            _snackbar.Add("This quote cannot be edited in its current status.", Severity.Warning);
            return;
        }

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

        _submitting = true;
        try
        {
            Result<int> res = await _apiService.UpdateQuote(_model);
            if (res is null || !res.IsSuccess)
            {
                _snackbar.Add(res?.ErrorDescription ?? "Failed to update quote.", Severity.Error);
                return;
            }

            _snackbar.Add("Quote updated.", Severity.Success);
            _navigationManager.NavigateTo($"/quotes/{res.Value}");
        }
        finally
        {
            _submitting = false;
        }
    }

    private void OnContactChanged(ListContactDto? value)
    {
        _jobContact = value;
        if (_model is not null)
            _model.ContactId = value?.ContactId ?? 0;
    }

    private void AddService()
    {
        if (_model is null || _serviceType is null)
            return;
        _model.QuoteItems.Add(new QuoteItemDto
        {
            Id = 0,
            ServiceName = _serviceType.ServiceName,
            ServiceTypeId = _serviceType.Id,
            Description = _description,
            Price = _price
        });
    }

    private void RemoveService(QuoteItemDto service)
    {
        _model?.QuoteItems.Remove(service);
    }

    private void SelectTemplate(QuoteTemplateDto template)
    {
        if (_model is null)
            return;
        _model.Description = template.Description;
        _model.QuoteItems = [.. (template.QuoteTemplateItems ?? []).Select(qi => new QuoteItemDto
        {
            Id = 0,
            Description = qi.Description,
            ServiceName = qi.ServiceName,
            ServiceTypeId = qi.ServiceTypeId,
            Price = qi.DefaultPrice
        })];
    }
}
