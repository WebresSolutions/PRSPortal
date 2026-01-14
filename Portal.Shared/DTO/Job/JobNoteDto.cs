using Portal.Shared.DTO.User;

namespace Portal.Shared.DTO.Job;

/// <summary>
/// Data transfer object representing a job note
/// Contains note content, creation date, and assigned user information
/// </summary>
public class JobNoteDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the note
    /// </summary>
    public int NoteId { get; set; }
    /// <summary>
    /// Gets or sets the content or text of the note
    /// </summary>
    public required string Content { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the note was created
    /// </summary>
    public DateTime DateCreated { get; set; }
    /// <summary>
    /// Gets or sets the user assigned to this note, if any
    /// </summary>
    public UserDto? AssignedUser { get; set; }
}
