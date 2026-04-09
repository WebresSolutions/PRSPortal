using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared.DTO;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Types;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Contact;

public partial class EditContact
{
    [Parameter]
    public int ContactId { get; set; }

    private ContactUpdateDto? _model;
    private ListContactDto? _parentContact;
    private ContactTypeDto[] _contactTypes = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadContactData();
        Result<ContactTypeDto[]>? typesResult = await _apiService.GetContactTypes();
        if (typesResult?.IsSuccess == true && typesResult.Value is not null)
            _contactTypes = typesResult.Value;

        _breadCrumbService.SetBreadCrumbItems(
        [
                new("Contacts", href: "/contacts", disabled: false),
                new("Edit Contact", href: $"/contacts/edit/{ContactId}", disabled: true)
        ]);
    }

    private async Task LoadContactData()
    {
        IsLoading = true;
        try
        {
            Result<ContactDetailsDto>? result = await _apiService.GetContactDetails(ContactId);
            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                ContactDetailsDto d = result.Value;
                _model = new ContactUpdateDto
                {
                    ContactId = d.ContactId,
                    TypeId = d.ContactType,
                    ParentContactId = d.ParentContact?.contactId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Email = d.Email,
                    Phone = d.Phone,
                    Fax = d.Fax,
                    Address = d.Address != null
                        ? new AddressDto(d.Address.AddressId, d.Address.State, d.Address.StateId, d.Address.Suburb ?? "", d.Address.Street ?? "", d.Address.PostCode ?? "")
                        {
                            LatLng = d.Address.LatLng ?? new LatLngDto(-37.8136, 144.9631)
                        }
                        : null
                };
                if (d.ParentContact is not null)
                    _parentContact = new ListContactDto(d.ParentContact.contactId, d.ParentContact.fullName, "", null, null, null, (ContactTypeEnum)d.ContactType);
            }
            else
            {
                _snackbar?.Add("Error loading contact details", Severity.Error);
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

    private void OnParentContactChanged(ListContactDto? value)
    {
        _parentContact = value;
        _model?.ParentContactId = value?.ContactId;
    }

    private async Task SubmitAsync()
    {
        if (_model is null) return;

        try
        {
            Result<ContactDetailsDto> result = await _apiService.UpdateContact(_model);
            if (result.IsSuccess && result.Value is not null)
            {
                _snackbar?.Add("Contact updated successfully.", Severity.Success);
                _navigationManager.NavigateTo($"/contacts/{result.Value.ContactId}");
            }
            else
                _snackbar?.Add(result.ErrorDescription ?? "Failed to update contact.", Severity.Error);
        }
        finally
        {

        }
    }

    public async Task<IEnumerable<ListContactDto>> SearchContacts(string search, CancellationToken token)
    {
        ContactFilterDto filter = new(Page: 1, PageSize: 500, SearchFilter: search, OrderBy: nameof(ListContactDto.FullName), Order: Portal.Shared.SortDirectionEnum.Desc);
        Result<PagedResponse<ListContactDto>>? contactResult = await _apiService.GetAllContacts(filter);
        if (contactResult?.IsSuccess == true && contactResult.Value?.Result is not null)
            return contactResult.Value.Result;
        return [];
    }
}
