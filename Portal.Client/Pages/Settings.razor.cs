using MudBlazor;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.DTO.Setting;
using Portal.Shared.ResponseModels;
using System.Text.Json;
using Toolbelt.Blazor.HotKeys2;

namespace Portal.Client.Pages;

/// <summary>
/// Blazor page component for application settings
/// Allows users to configure theme colors, schedule colors, and hotkeys
/// </summary>
public partial class Settings
{
    /// <summary>
    /// Gets or sets the list of available schedule colors
    /// </summary>
    private List<ScheduleColourDto> _colours = [];
    /// <summary>
    /// Gets or sets the primary theme color
    /// </summary>
    private string primaryColour = "#1976d2";
    /// <summary>
    /// Gets or sets the secondary theme color
    /// </summary>
    private string secondaryColour = "#1976d2";
    /// <summary>
    /// Gets or sets the array of hotkey entries configured for the application
    /// </summary>
    private HotKeyEntry[] hotKeyEntries = [];

    /// <summary>
    /// Called when the component is initialized.
    /// Data loading for the grid is now handled by LoadFacilitiesServerData.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    protected override async Task OnInitializedAsync()
    {
        base.IsLoading = true;
        await base.OnInitializedAsync();
        primaryColour = await GetColour("--color-primary");
        hotKeyEntries = base._hotKeysContext?.HotKeyEntries.ToArray() ?? [];
        await LoadColours();
        base.IsLoading = false;
    }

    /// <summary>
    /// Loads the schedule colors from the server
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task LoadColours()
    {
        base.IsLoading = true;
        Result<List<ScheduleColourDto>> res = await _apiService.GetScheduleColours();
        if (res.Error is null && res.Value is not null)
        {
            _colours = res.Value;
        }
        else
        {
            _snackbar.Add(res.ErrorDescription ?? "Error occured while loading the colours", Severity.Error);
        }
        base.IsLoading = false;
    }

    /// <summary>
    /// Saves a schedule color update to the server
    /// </summary>
    /// <param name="colour">The schedule color DTO to update</param>
    /// <param name="colourString">The new color hex value</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task SaveColour(ScheduleColourDto colour, string colourString)
    {
        colour.ColourHex = colourString;
        Result<ScheduleColourDto> res = await _apiService.UpdateScheduleColour(colour);
        if (res.Error is null)
        {
            _snackbar.Add("Colour updated successfully", Severity.Success);
            Result<List<ScheduleColourDto>> colours = await _apiService.GetScheduleColours();
            if (colours.Error is null && colours.Value is not null)
            {
                _colours.ElementAt(_colours.FindIndex(c => c.ScheduleColourId == colour.ScheduleColourId)).ColourHex = colourString;
            }
            else
            {
                _snackbar.Add(res.ErrorDescription ?? "Error occured while loading the colours", Severity.Error);
            }
        }
        else
        {
            _snackbar.Add(res.ErrorDescription ?? "Error occured while saving the colour", Severity.Error);
        }
    }

    /// <summary>
    /// Sets the primary or secondary theme color and persists it to the server
    /// Updates the CSS variable and saves the setting
    /// </summary>
    /// <param name="colour">The color hex value to set</param>
    /// <param name="primary">True to set primary color, false for secondary</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task SetColour(string colour, bool primary)
    {
        if (colour == primaryColour && primary || colour == secondaryColour && !primary)
            return;

        if (primary)
            primaryColour = colour;
        else
            secondaryColour = colour;

        await Helpers.SetColour(_jsRuntime, colour, primary);

        ThemeSettingsDto settingsObject = new()
        {
            PrimaryColour = primaryColour,
            SecondaryColour = secondaryColour
        };

        _sessionStorage.SetItem("SystemSettings", settingsObject);
        string settingsJson = JsonSerializer.Serialize(settingsObject);

        SystemSettingDto obj = new() { SettingJson = settingsJson };
        Result<SystemSettingDto> result = await _apiService.UpdateSystemSettings(obj);

        if (result.Error is null)
            _snackbar.Add("Theme settings updated successfully", Severity.Success);
        else
            _snackbar.Add(result.ErrorDescription ?? "Error occurred while saving theme settings", Severity.Error);
    }

    /// <summary>
    /// Gets the current value of a CSS color variable
    /// </summary>
    /// <param name="variableName">The name of the CSS variable (e.g., "--color-primary")</param>
    /// <returns>The current color value of the CSS variable</returns>
    private async Task<string> GetColour(string variableName) => await Helpers.GetColour(_jsRuntime, variableName);

}