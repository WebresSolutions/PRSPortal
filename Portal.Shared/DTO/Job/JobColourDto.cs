namespace Portal.Shared.DTO.Job;

/// <summary>
/// Data transfer object representing a job color
/// Contains color identification and value
/// </summary>
/// <param name="Id">The unique identifier for the job color</param>
/// <param name="colour">The color value (typically a hex code)</param>
public record JobColourDto(int Id, string colour);
