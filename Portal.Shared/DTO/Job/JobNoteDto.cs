using System.ComponentModel.DataAnnotations;
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
    /// Gets or sets the unique identifier of the job associated with this note
    /// </summary>
    public int JobId { get; set; }
    /// <summary>
    /// Gets or sets the content or text of the note
    /// </summary>
    [MaxLength(4000, ErrorMessage = "Note content cannot exceed 4000 characters")]
    public required string Content { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the note was created
    /// </summary>
    public DateTime DateCreated { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the entity was updated
    /// </summary>
    public DateTime? DateModified { get; set; }
    /// <summary>
    /// Gets or sets the user assigned to this note, if any
    /// </summary>
    public UserDto? AssignedUser { get; set; }
    /// <summary>
    /// Gets or sets if action is required
    /// </summary>
    public bool ActionRequired { get; set; }
    /// <summary>
    /// If the note has been marked as deleted. Deleted notes are typically not shown in the UI but are kept in the database for record-keeping purposes.
    /// </summary>
    public bool Deleted { get; set; }
    /// <summary>
    /// The created by
    /// </summary>
    public string? CreatedBy { get; set; }
    /// <summary>
    /// The created by
    /// </summary>
    public string? Modifiedby { get; set; }
}
