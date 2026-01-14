using Portal.Shared.DTO.Setting;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

/// <summary>
/// Interface for system settings management operations
/// Defines methods for retrieving and updating application settings
/// </summary>
public interface ISettingService
{
    /// <summary>
    /// Retrieves the current system settings
    /// </summary>
    /// <returns>A result containing the system settings DTO</returns>
    Task<Result<SystemSettingDto>> GetSystemSettings();
    /// <summary>
    /// Updates the system settings with new values
    /// </summary>
    /// <param name="settingsDto">The system settings DTO containing the updated values</param>
    /// <returns>A result containing the updated system settings DTO</returns>
    Task<Result<SystemSettingDto>> UpdateSystemSettings(SystemSettingDto settingsDto);
}