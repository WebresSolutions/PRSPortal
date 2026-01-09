namespace Portal.Shared.DTO.User;

/// <summary>
/// Data transfer object representing user information
/// Contains user identification and display name
/// </summary>
/// <param name="userId">The unique identifier for the user</param>
/// <param name="displayName">The display name of the user</param>
public record UserDto(int? userId, string? displayName);
