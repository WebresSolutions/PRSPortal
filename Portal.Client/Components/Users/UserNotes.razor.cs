using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Client.Components.JobComponents;
using Portal.Client.Components.UIComponents;
using Portal.Shared;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Components.Users;

public partial class UserNotes
{
    public JobNoteDto[] Notes { get; set; } = [];

    /// <summary>
    /// The job id that these notes belong to. Required for creating a new note when the notes list is empty.
    /// </summary>
    [Parameter]
    public int JobId { get; set; }
    /// <summary>
    /// The search string for searching for certain notes
    /// </summary>
    private string? SearchString = null;
    /// <summary>
    /// A copy of the original notes list used for filtering and searching. 
    /// This allows us to maintain the original list of notes while applying filters or search criteria to the displayed list without losing the original data.
    /// </summary>
    private JobNoteDto[] NotesCopy { get; set; } = [];
    /// <summary>
    /// A list of tabs for filtering notes based on their status (e.g., All, Action Required, Deleted).
    /// </summary>
    private HashSet<TabTypeEnum> Tabs { get; set; } = [TabTypeEnum.All, TabTypeEnum.ActionRequired, TabTypeEnum.Deleted];
    /// <summary>
    /// The card reference
    /// </summary>
    private Card? _cardRef;
    /// <summary>
    /// When parameters are set or changed, syncs the notes list from the API for the current JobId.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await SyncNotesFromParameter();
    }

    /// <summary>
    /// Syncs the notes list from the component parameter (JobId) by fetching notes from the API.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SyncNotesFromParameter()
    {
        Result<JobNoteDto[]> notesResult = await _apiService.GetUserNotes();
        if (notesResult.IsSuccess && notesResult.Value is not null)
        {
            Notes = notesResult.Value;
            NotesCopy = notesResult.Value;
        }
    }

    /// <summary>
    /// When searching through notes, this method is used to highlight the search string within the note content. It takes a note's content as input and checks if it contains the search string (case-insensitive). If it does, it wraps the matching portion of the text in a <mark> HTML tag, which typically highlights the text with a background color. 
    /// The method returns a MarkupString that can be rendered in the UI, allowing users to easily identify where their search term appears within each note.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private MarkupString GetHighlightedText(string text)
    {
        if (string.IsNullOrWhiteSpace(SearchString))
            return new MarkupString(text);

        int index = text.IndexOf(SearchString, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
            return new MarkupString(text);

        /// Get the before after and match parts of the text. Then put them together with the match wrapped in a <mark> tag to highlight it
        string before = text[..index];
        string match = text.Substring(index, SearchString.Length);
        string after = text[(index + SearchString.Length)..];

        // Wrap the match in a <mark> tag
        string highlighted = $"{before}<mark>{match}</mark>{after}";
        return new MarkupString(highlighted); // Return as MarkupString
    }

    /// <summary>
    /// Formats the note's creation date for display (e.g. "Monday 3-Mar-2025 2:30PM").
    /// </summary>
    /// <param name="note">The job note.</param>
    /// <returns>A formatted date/time string.</returns>
    private static string GetNoteDate(JobNoteDto note) =>
        note.DateCreated.ToLocalTime().ToString("dddd d-MMM-yyyy h:mmtt");

    /// <summary>
    /// Opens the dialog for editing a note.
    /// </summary>
    private async Task OpenDialogAsync(JobNoteDto note)
    {
        DialogParameters parameter = new DialogParameters<JobNoteDto> { { "Note", note } };
        DialogOptions options = new() { CloseButton = false, CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large };
        IDialogReference dialogRef = await _dialog.ShowAsync<EditJobNote>("", parameter, options);
        DialogResult? result = await dialogRef.Result;

        if (result is not null && result.Data is List<JobNoteDto> jobNoteDtos)
        {
            _cardRef?.SetSelectedTab(TabTypeEnum.All);
            await ChangeTabs(TabTypeEnum.All);
        }
    }

    /// <summary>
    /// Searches notes for notes containing a certain string in their content and updates the displayed notes list accordingly. This method is typically called when the user enters a search term in a search input field. It filters the original list of notes (NotesCopy) to find those that contain the search string in their content and updates the Notes list to display only the matching notes. This allows users to quickly find specific notes based on their content. If the search string is empty or null, it can reset the displayed notes to show all notes.
    /// </summary>
    /// <param name="search"></param>
    private void SearchNotes(string search) => Notes = [.. NotesCopy.Where(x => x.Content.Contains(search))];

    /// <summary>
    /// Clear the search
    /// </summary>
    private void ClearSearch() => Notes = NotesCopy;

    /// <summary>
    /// Delete a note
    /// </summary>
    /// <returns></returns>
    private async Task DeleteNoteAsync(JobNoteDto note)
    {
        note.Deleted = !note.Deleted;
        _ = await _apiService.SaveJobNote(note);
        await ChangeTabs(TabTypeEnum.All);
    }

    /// <summary>
    /// On tabs change filter the notes based on the selected tab
    /// </summary>
    /// <param name="Tab"></param>
    private async Task ChangeTabs(TabTypeEnum Tab)
    {
        try
        {
            SearchString = "";
            ClearSearch();
            switch (Tab)
            {
                case TabTypeEnum.All:
                    Handlenotes(await _apiService.GetUserNotes(false));
                    break;
                case TabTypeEnum.ActionRequired:
                    Handlenotes(await _apiService.GetUserNotes(false, true));
                    break;
                case TabTypeEnum.Deleted:
                    Handlenotes(await _apiService.GetUserNotes(true));
                    break;
                default:
                    break;
            }

            JobNoteDto[] Handlenotes(Result<JobNoteDto[]> notes)
            {
                if (notes.IsSuccess && notes.Value is not null)
                {
                    Notes = notes.Value;
                    NotesCopy = notes.Value;
                    return notes.Value;
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
}