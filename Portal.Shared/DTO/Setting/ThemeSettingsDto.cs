using System.Text.Json.Serialization;

namespace Portal.Shared.DTO.Setting;

public class ThemeSettingsDto
{
    [JsonPropertyName("primaryColour")]
    public string PrimaryColour { get; set; } = "#1976d2";
    
    [JsonPropertyName("secondaryColour")]
    public string SecondaryColour { get; set; } = "#1976d2";
}

