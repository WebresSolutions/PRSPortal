using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared;
using Portal.Shared.DTO.Contact;

namespace Portal.Client.Components.JobComponents;

public partial class TechnicalContacts
{
    [Parameter]
    public required IEnumerable<TechnicalContactDto> Contacts { get; set; }

    [Parameter]
    public int JobId { get; set; }

    private void ChangeTabs(TabTypeEnum tab)
    {
    }

    /// <summary>
    /// Opens the edit dialog to create a new note; on save, updates the displayed notes list.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task NewTechnicalContact(TechnicalContactDto? techContact)
    {
        SaveTechnicalContactTypeDto createContactDto;
        if (techContact is null)
            createContactDto = new SaveTechnicalContactTypeDto() { JobId = JobId };
        else
            createContactDto = new SaveTechnicalContactTypeDto(techContact);

        DialogParameters parameter = new DialogParameters<SaveTechnicalContactTypeDto> { { "Model", createContactDto } };
        DialogOptions options = new() { CloseButton = true, CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large };
        IDialogReference dialogRef = await _dialog.ShowAsync<EditTechnicalContact>("", parameter, options);
        DialogResult? result = await dialogRef.Result;
    }
}