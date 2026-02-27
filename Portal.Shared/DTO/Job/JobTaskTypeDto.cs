namespace Portal.Shared.DTO.Job;

/// <summary>
/// Data transfer object for job task type lookup.
/// </summary>
public record JobTaskTypeDto(int Id, string Name, string? Description);
