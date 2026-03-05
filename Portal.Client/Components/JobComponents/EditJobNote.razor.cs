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

    private MudExRichTextEdit _editor = null!;

    private string _saveContent = "";

    private bool _readonly = false;

    private int? _assignedUser;

    /// <summary>
    /// Loads users for assignment and initializes the editor with the note content when parameters are set.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnParametersSetAsync()
    {

        Result<UserDto[]> usersResult = await _apiService.GetUsersList();
        if (usersResult.IsSuccess)
            Users = usersResult.Value!;
        else
            Snackbar.Add("Failed to load users for note assignment.", Severity.Error);

        if (Note.AssignedUser is not null)
            _assignedUser = Users.FirstOrDefault(u => u.userId == Note.AssignedUser.userId)?.userId;

        await Task.Delay(300);

        if (!string.IsNullOrEmpty(Note.Content))
        {
            Console.WriteLine($"[Debug] Content Length: {Note.Content.Length}");

            // Check for massive Base64 strings that cause 30s hangs
            if (Note.Content.Contains("data:image") && Note.Content.Length > 100000)
            {
                Snackbar.Add("Large images detected. Performance may be affected.", Severity.Warning);
                _saveContent = Note.Content;
            }
            else
            {
                _saveContent = Note.Content;
            }
        }

        StateHasChanged();
    }

    /// <summary>
    /// Handles rich text editor content changes and stores the value for saving.
    /// </summary>
    /// <param name="value">The new content from the editor.</param>
    private void OnContentChanged(string value)
    {
        _saveContent = value;
        // Note.Content = value;
    }

    /// <summary>
    /// Saves the note content and optional assignment to the API and closes the dialog on success.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SaveNote()
    {
        Note.Content = _saveContent;
        if (_assignedUser != Note.AssignedUser?.userId)
            Note.AssignedUser = _assignedUser is null ? null : new UserDto(_assignedUser, Users.FirstOrDefault(u => u.userId == _assignedUser)?.displayName);

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