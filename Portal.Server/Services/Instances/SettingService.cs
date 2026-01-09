using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.Setting;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Instances;

/// <summary>
/// Service implementation for system settings management
/// Handles retrieval and updates of application-wide settings
/// </summary>
public class SettingService(PrsDbContext _dbContext, ILogger<SettingService> _logger) : ISettingService
{
    /// <summary>
    /// Retrieves the current system settings from the database
    /// Creates default settings if none exist
    /// </summary>
    /// <returns>A result containing the system settings DTO</returns>
    public async Task<Result<SystemSettingDto>> GetSystemSettings()
    {
        Result<SystemSettingDto> res = new();
        const string settingsKey = "SystemSettings";
        ApplicationSetting? settings = await _dbContext.ApplicationSettings.FirstOrDefaultAsync(x => x.Key == settingsKey);

        if (settings is null)
        {
            // create new
            settings = new ApplicationSetting
            {
                Key = settingsKey,
                Value = "{}",
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            _dbContext.ApplicationSettings.Add(settings);
            await _dbContext.SaveChangesAsync();
        }

        res.Value = new SystemSettingDto
        {
            SettingJson = settings.Value
        };

        return res;
    }

    /// <summary>
    /// Updates the system settings in the database
    /// </summary>
    /// <param name="settingsDto">The system settings DTO containing the updated settings</param>
    /// <returns>A result containing the updated system settings DTO</returns>
    public async Task<Result<SystemSettingDto>> UpdateSystemSettings(SystemSettingDto settingsDto)
    {
        Result<SystemSettingDto> res = new();
        const string settingsKey = "SystemSettings";
        ApplicationSetting? settings = await _dbContext.ApplicationSettings.FirstOrDefaultAsync(x => x.Key == settingsKey);

        if (settings is null)
            return res.SetError(ErrorType.NotFound, "System settings not found");

        settings.ModifiedAt = DateTime.UtcNow;
        settings.Value = settingsDto.SettingJson; // TODO: update with 
        await _dbContext.SaveChangesAsync();

        res.Value = new SystemSettingDto
        {
            SettingJson = settings.Value
        };

        return res;
    }

}
