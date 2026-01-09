using Portal.Shared.DTO.Setting;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

public interface ISettingService
{
    Task<Result<SystemSettingDto>> GetSystemSettings();
    Task<Result<SystemSettingDto>> UpdateSystemSettings(SystemSettingDto settingsDto);
}