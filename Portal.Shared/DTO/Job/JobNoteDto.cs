using Portal.Shared.DTO.User;

namespace Portal.Shared.DTO.Job;

public class JobNoteDto
{
    public int NoteId { get; set; }
    public required string Content { get; set; }
    public DateTime DateCreated { get; set; }
    public UserDto? AssignedUser { get; set; }
}
