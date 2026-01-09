namespace Portal.Shared.DTO.Setting;

/// <summary>
/// Data transfer object representing system settings
/// Contains JSON-serialized settings data
/// </summary>
public class SystemSettingDto
{
    /// <summary>
    /// Gets or sets the JSON string containing system settings
    /// </summary>
    public string SettingJson { get; set; } = string.Empty;
}
