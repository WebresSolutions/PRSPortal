using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared;
using Portal.Shared.DTO.Contact;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Components.JobComponents;

public partial class EditTechnicalContact
{

    /// <summary>
    /// The Model for the technical contact being edited or created
    /// </summary>
    [Parameter]
    public required SaveTechnicalContactTypeDto Model { get; set; }
    /// <summary>
    /// The List of job contacts
    /// </summary>
    private ListContactDto? _jobContact = null!;
    /// <summary>
    /// List of contact types for the dropdown
    /// </summary>
    private TechnicalContactTypeDto[] _contactTypes = [];
    /// <summary>
    /// If adding a new contact, the header will show "Add Technical Contact", if editing an existing contact, it will show "Edit Technical Contact"
    /// </summary>
    private string HeaderText => Model.Id == 0 ? "Add Technical Contact" : "Edit Technical Contact";

    /// <summary>
    /// Called when the component is initialized, used to load the contact types for the dropdown
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        base.IsLoading = true;
        Result<TechnicalContactTypeDto[]>? contactTypesResult = await _apiService.GetTechnicalContactTypes();
        if (contactTypesResult?.IsSuccess == true && contactTypesResult.Value is not null)
            _contactTypes = contactTypesResult.Value;

        if (Model.Id is not 0)
        {
            _jobContact = new()
            {
                FullName = Model.ContactName ?? "",
                Email = Model.Email ?? "",
                Phone = Model.Phone ?? "",
            };
        }
        base.IsLoading = false;
    }

    /// <summary>
    /// Called when the contact is changed, used to update the model
    /// </summary>
    /// <param name="value">The new contact</param>
    /// <returns></returns>
    private void OnContactChanged(ListContactDto value)
    {
        _jobContact = value;
        Model.ContactId = value.ContactId;
    }

    /// <summary>
    /// Called when the form is submitted, used to save the technical contact
    /// </summary>
    /// <returns></returns>
    private async Task SubmitAsync()
    {
        Result<TechnicalContactDto[]> result = await _apiService.SaveTechnicalContact(Model);
        if (!result.IsSuccess)
        {
            _snackbar.Add("Failed to Save Technical Contact.", MudBlazor.Severity.Error);
            base.MudDialog?.Close(DialogResult.Ok(false));
        }
        else
        {
            _snackbar.Add("Saved Contact Successfully.", MudBlazor.Severity.Success);
            base.MudDialog?.Close(DialogResult.Ok(true));
        }
    }

    /// <summary>
    /// Used by the type ahead auto complete for searching contacts
    /// </summary>
    /// <param name="search">The search string</param>
    /// <param name="token">The token</param>
    /// <returns></returns>
    public async Task<IEnumerable<ListContactDto>> SearchContacts(string search, CancellationToken token)
    {
        Result<PagedResponse<ListContactDto>>? contactResult = await _apiService.GetAllContacts(100, 1, search, null, SortDirectionEnum.Asc);

        if (contactResult?.IsSuccess == true && contactResult.Value?.Result is not null)
            return contactResult.Value.Result;
        else
            return [];
    }
}