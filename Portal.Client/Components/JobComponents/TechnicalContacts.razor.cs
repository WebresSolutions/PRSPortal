using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Client.Components.UIComponents;
using Portal.Shared;
using Portal.Shared.DTO.Contact;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Components.JobComponents;

public partial class TechnicalContacts
{
    /// <summary>
    /// The job id that these technical contacts belong to
    /// </summary>
    [Parameter]
    public int? JobId { get; set; }
    /// <summary>
    /// The job id that these technical contacts belong to
    /// </summary>
    [Parameter]
    public int? ContactId { get; set; }

    /// <summary>
    /// The card reference
    /// </summary>
    private Card? _cardRef;

    /// <summary>
    /// The list of technical contacts
    /// </summary>
    private TechnicalContactDto[] TechnicalContactDtos = [];

    /// <summary>
    /// Called when the component is initialized, used to load the technical contacts
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        base.IsLoading = true;
        await base.OnInitializedAsync();

        Result<TechnicalContactDto[]> contactsResult = await _apiService.GetTechnicalContacts(JobId, ContactId, false);
        if (contactsResult.IsSuccess)
            TechnicalContactDtos = contactsResult.Value ?? [];
        else
            _snackbar.Add(contactsResult.ErrorDescription ?? "Failed to load technical contacts.", Severity.Error);

        base.IsLoading = false;
    }

    /// <summary>
    /// On tabs change filter the technical contacts based on the selected tab
    /// </summary>
    /// <param name="tab">The tab to change to</param>
    /// <returns></returns>
    private async Task ChangeTabs(TabTypeEnum tab)
    {
        try
        {
            switch (tab)
            {
                case TabTypeEnum.All:
                    HandleData(await _apiService.GetTechnicalContacts(JobId, ContactId, false));
                    break;

                case TabTypeEnum.Deleted:
                    HandleData(await _apiService.GetTechnicalContacts(JobId, ContactId, true));
                    break;
                default:
                    break;
            }

            TechnicalContactDto[] HandleData(Result<TechnicalContactDto[]> data)
            {
                if (data.IsSuccess && data.Value is not null)
                {
                    TechnicalContactDtos = data.Value;
                    return data.Value;
                }
                _snackbar.Add("Failed to load notes", Severity.Error);
                return [];
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Deletes the specified technical contact from the system asynchronously.
    /// </summary>
    /// <param name="techContact">The technical contact to be deleted. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    private async Task DeletedContact(TechnicalContactDto techContact)
    {
        SaveTechnicalContactTypeDto createContactDto = new(techContact)
        {
            Deleted = !techContact.Deleted
        };

        Result<TechnicalContactDto[]> res = await _apiService.SaveTechnicalContact(createContactDto);
        if (res.IsSuccess)
            await SetAllTab();
    }

    /// <summary>
    /// Opens the edit dialog to create a new note; on save, updates the displayed notes list.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task NewTechnicalContact(TechnicalContactDto? techContact)
    {
        if (JobId is null)
            return;

        SaveTechnicalContactTypeDto createContactDto;
        if (techContact is null)
            createContactDto = new SaveTechnicalContactTypeDto() { JobId = JobId.Value };
        else
            createContactDto = new SaveTechnicalContactTypeDto(techContact);

        DialogParameters parameter = new DialogParameters<SaveTechnicalContactTypeDto> { { "Model", createContactDto } };
        DialogOptions options = new() { CloseButton = true, CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large };
        IDialogReference dialogRef = await _dialog.ShowAsync<EditTechnicalContact>("", parameter, options);
        DialogResult? res = await dialogRef.Result;

        if (res?.Data is bool resValue && resValue is true)
            await SetAllTab();
    }
    async Task SetAllTab()
    {
        _cardRef?.SetSelectedTab(TabTypeEnum.All);
        await ChangeTabs(TabTypeEnum.All);
    }
}