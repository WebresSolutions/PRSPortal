using Portal.Shared.DataEnums;

namespace Portal.Shared.DTO.User;

/// <summary>
/// Holds the user assignment information
/// </summary>
public record UserAssignmentDto(int UserId, int JobId, JobAssignementTypeEnum assignmentType);
