using System.Text.Json.Serialization;

namespace Portal.Shared.DTO.Setting;

/// <summary>
/// Data transfer object representing theme color settings
/// Contains primary and secondary color configuration
/// </summary>
public class ThemeSettingsDto
{
    /// <summary>
    /// Gets or sets the primary theme color in hexadecimal format
    /// </summary>
    [JsonPropertyName("primaryColour")]
    public string PrimaryColour { get; set; } = "#1976d2";
    
    /// <summary>
    /// Gets or sets the secondary theme color in hexadecimal format
    /// </summary>
    [JsonPropertyName("secondaryColour")]
    public string SecondaryColour { get; set; } = "#1976d2";
}

