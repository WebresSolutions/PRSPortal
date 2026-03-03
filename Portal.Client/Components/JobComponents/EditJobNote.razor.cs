using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudExRichTextEditor;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Components.JobComponents;

public partial class EditJobNote
{
    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }

    [Parameter]
    public EventCallback OnNoteSaved { get; set; }

    [Parameter]
    public required JobNoteDto Note { get; set; }

    /// <summary>
    /// List of users to assign the note to. Loaded from the API when the component parameters are set. Used to populate the user assignment dropdown in the UI.
    /// </summary>
    public UserDto[] Users { get; set; } = [];

    private MudExRichTextEdit Editor = null!;

    private string SaveContent = "";

    private bool Readonly = false;

    private int? AssignedUser;

    protected override async Task OnParametersSetAsync()
    {

        Result<UserDto[]> usersResult = await _apiService.GetUsersList();
        if (usersResult.IsSuccess)
            Users = usersResult.Value!;
        else
            Snackbar.Add("Failed to load users for note assignment.", Severity.Error);

        if (Note.AssignedUser is not null)
            AssignedUser = Users.FirstOrDefault(u => u.userId == Note.AssignedUser.userId)?.userId;

        await Task.Delay(300);

        if (!string.IsNullOrEmpty(Note.Content))
        {
            Console.WriteLine($"[Debug] Content Length: {Note.Content.Length}");

            // Check for massive Base64 strings that cause 30s hangs
            if (Note.Content.Contains("data:image") && Note.Content.Length > 100000)
            {
                Snackbar.Add("Large images detected. Performance may be affected.", Severity.Warning);
                SaveContent = Note.Content;
            }
            else
            {
                SaveContent = Note.Content;
            }
        }

        StateHasChanged();
    }

    private void OnContentChanged(string value)
    {
        SaveContent = value;
        // Note.Content = value;
    }

    private async Task SaveNote()
    {
        Note.Content = SaveContent;
        if (AssignedUser != Note.AssignedUser?.userId)
            Note.AssignedUser = AssignedUser is null ? null : new UserDto(AssignedUser, Users.FirstOrDefault(u => u.userId == AssignedUser)?.displayName);

        Result<List<JobNoteDto>> result = await _apiService.SaveJobNote(Note);
        if (result.IsSuccess && result.Value is not null)
        {
            Snackbar.Add("Note saved successfully.", Severity.Success);
            MudDialog?.Close(DialogResult.Ok(result.Value));
        }
        else
        {
            Snackbar.Add(result.ErrorDescription ?? "Failed to save note.", Severity.Error);
        }
    }

}