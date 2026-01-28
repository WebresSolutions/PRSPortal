using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.Setting;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Controllers;

/// <summary>
/// Static class containing settings-related API endpoint definitions
/// Provides RESTful endpoints for system settings operations
/// </summary>
public static class SettingsEndpoints
{
    /// <summary>
    /// Registers settings-related API endpoints with the application
    /// </summary>
    /// <param name="app">The web application to register endpoints with</param>
    public static void AddSettingEndpoints(this WebApplication app, bool reqAuth = true)
    {
        RouteGroupBuilder appGroup = app.MapGroup("/api/settings");

        // Gets all jobs with pagination and optional filtering/sorting
        appGroup.MapGet("systemsettings", async (
            [FromServices] ISettingService setService,
            HttpContext httpContext
            ) =>
        {
            Result<SystemSettingDto> result = await setService.GetSystemSettings();
            return EndpointsHelper.ProcessResult(result, "An Error occured while getting system settings");
        });

        appGroup.MapPut("systemsettings", async (
            [FromServices] ISettingService setService,
            [FromBody] SystemSettingDto settingsDto,
            HttpContext httpContext
            ) =>
        {
            Result<SystemSettingDto> result = await setService.UpdateSystemSettings(settingsDto);
            return EndpointsHelper.ProcessResult(result, "An Error occured while saving system settings");
        });

        if (reqAuth)
            appGroup.RequireAuthorization();
        else
            appGroup.AllowAnonymous();
    }
}
