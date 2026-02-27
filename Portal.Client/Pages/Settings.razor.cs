using MudBlazor;
using Portal.Client.Components.Settings;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.DTO.Setting;
using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.ResponseModels;
using System.Text.Json;
using Toolbelt.Blazor.HotKeys2;

namespace Portal.Client.Pages;

/// <summary>
/// Blazor page component for application settings
/// Allows users to configure theme colors, schedule colors, type lists, and hotkeys
/// </summary>
public partial class Settings
{
    private List<ScheduleColourDto> _colours = [];
    private string primaryColour = "#1976d2";
    private string secondaryColour = "#1976d2";
    private HotKeyEntry[] hotKeyEntries = [];

    private List<TimeTypeDto> _timesheetTypes = [];
    private List<ContactTypeDto> _contactTypes = [];
    private List<JobTypeDto> _jobTypes = [];
    private List<JobColourDto> _jobColours = [];
    private List<FileTypeDto> _fileTypes = [];
    private List<JobTaskTypeDto> _jobTaskTypes = [];
    private List<TechnicalContactTypeDto> _technicalContactTypes = [];
    private List<StateDto> _states = [];

    protected override async Task OnInitializedAsync()
    {
        base.IsLoading = true;
        await base.OnInitializedAsync();
        primaryColour = await GetColour("--color-primary");
        secondaryColour = await GetColour("--color-secondary");
        await LoadColours();
        await LoadAllTypes();
        base.IsLoading = false;
    }

    private async Task LoadAllTypes()
    {
        await Task.WhenAll(
            LoadTimesheetTypes(),
            LoadContactTypes(),
            LoadJobTypes(),
            LoadJobColours(),
            LoadFileTypes(),
            LoadJobTaskTypes(),
            LoadTechnicalContactTypes(),
            LoadStates());
    }

    private async Task LoadTimesheetTypes()
    {
        Result<TimeTypeDto[]> res = await _apiService.GetTimeSheetTypes();
        if (res.IsSuccess && res.Value is { } v) _timesheetTypes = v.ToList();
    }
    private async Task LoadContactTypes()
    {
        Result<ContactTypeDto[]> res = await _apiService.GetContactTypes();
        if (res.IsSuccess && res.Value is { } v) _contactTypes = v.ToList();
    }
    private async Task LoadJobTypes()
    {
        Result<JobTypeDto[]> res = await _apiService.GetJobTypes();
        if (res.IsSuccess && res.Value is { } v) _jobTypes = v.ToList();
    }
    private async Task LoadJobColours()
    {
        Result<JobColourDto[]> res = await _apiService.GetJobColours();
        if (res.IsSuccess && res.Value is { } v) _jobColours = v.ToList();
    }
    private async Task LoadFileTypes()
    {
        Result<FileTypeDto[]> res = await _apiService.GetFileTypes();
        if (res.IsSuccess && res.Value is { } v) _fileTypes = v.ToList();
    }
    private async Task LoadJobTaskTypes()
    {
        Result<JobTaskTypeDto[]> res = await _apiService.GetJobTaskTypes();
        if (res.IsSuccess && res.Value is { } v) _jobTaskTypes = v.ToList();
    }
    private async Task LoadTechnicalContactTypes()
    {
        Result<TechnicalContactTypeDto[]> res = await _apiService.GetTechnicalContactTypes();
        if (res.IsSuccess && res.Value is { } v) _technicalContactTypes = v.ToList();
    }
    private async Task LoadStates()
    {
        Result<StateDto[]> res = await _apiService.GetStates();
        if (res.IsSuccess && res.Value is { } v) _states = v.ToList();
    }

    private async Task OpenEditTypeDialog(string typeName, string title, object? item)
    {
        var parameters = new DialogParameters
        {
            [nameof(EditTypeDialog.TypeName)] = typeName,
            [nameof(EditTypeDialog.Title)] = title,
            [nameof(EditTypeDialog.Item)] = item,
            [nameof(EditTypeDialog.ApiService)] = _apiService,
            [nameof(EditTypeDialog.Snackbar)] = _snackbar
        };
        var options = new DialogOptions { CloseOnEscapeKey = true };
        IDialogReference dialog = await _dialog.ShowAsync<EditTypeDialog>(title, parameters, options);
        DialogResult? result = await dialog.Result;
        if (result is { Canceled: false })
            await LoadAllTypes();
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

        SystemSettingsDto settingsObject = new()
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