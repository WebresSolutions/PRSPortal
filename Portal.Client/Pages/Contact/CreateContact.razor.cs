using MudBlazor;
using Nextended.Core.Extensions;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Types;
using Portal.Shared.ResponseModels;
using System.ComponentModel.DataAnnotations;

namespace Portal.Client.Pages.Contact;

public partial class CreateContact
{
    private ContactCreationDto _model = null!;
    private ListContactDto? _parentContact;
    private ContactTypeDto[] _contactTypes = [];
    private bool _submitting;

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await base.OnInitializedAsync();
        _model = new()
        {
            TypeId = 1,
            Address = new()
            {
                LatLng = new(-37.8136, 144.9631)
            }
        };

        Result<ContactTypeDto[]>? typesResult = await _apiService.GetContactTypes();
        if (typesResult?.IsSuccess == true && typesResult.Value is not null)
            _contactTypes = typesResult.Value;

        IsLoading = false;

        _breadCrumbService.SetBreadCrumbItems(
        [
                new("Contacts", href: "/contacts", disabled: false),
                new("Create Contact", href: "/contacts/create", disabled: true)
        ]);
    }

    private void OnParentContactChanged(ListContactDto? value)
    {
        _parentContact = value;
        _model.ParentContactId = value?.ContactId;
    }

    private async Task SubmitAsync()
    {
        _submitting = true;
        try
        {
            IEnumerable<ValidationResult> res = _model.Validate();
            if (res.IsEmpty())
            {
                Result<int> result = await _apiService.CreateContact(_model);
                if (result.IsSuccess && result.Value > 0)
                {
                    _snackbar?.Add("Contact created successfully.", Severity.Success);
                    _navigationManager.NavigateTo($"/contacts/{result.Value}");
                }
                else
                    _snackbar?.Add(result.ErrorDescription ?? "Failed to create contact.", Severity.Error);
            }
            else
            {
                _snackbar?.Add(res.Select(x => x.ErrorMessage).JoinWith(" "), Severity.Error);
            }
        }
        finally
        {
            _submitting = false;
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
