using MudBlazor;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.DTO.Setting;
using Portal.Shared.ResponseModels;
using System.Text.Json;
using Toolbelt.Blazor.HotKeys2;

namespace Portal.Client.Pages;

public partial class Settings
{
    private List<ScheduleColourDto> _colours = [];
    private string primaryColour = "#1976d2";
    private string secondaryColour = "#1976d2";
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

    private async Task<string> GetColour(string variableName) => await Helpers.GetColour(_jsRuntime, variableName);

}