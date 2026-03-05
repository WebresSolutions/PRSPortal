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
    private string _primaryColour = "#1976d2";
    private string _secondaryColour = "#1976d2";
    private HotKeyEntry[] _hotKeyEntries = [];

    private List<TimeTypeDto> _timesheetTypes = [];
    private List<ContactTypeDto> _contactTypes = [];
    private List<JobTypeDto> _jobTypes = [];
    private List<JobColourDto> _jobColours = [];
    private List<FileTypeDto> _fileTypes = [];
    private List<JobTaskTypeDto> _jobTaskTypes = [];
    private List<TechnicalContactTypeDto> _technicalContactTypes = [];
    private List<StateDto> _states = [];

    /// <summary>
    /// Loads theme colours, schedule colours, and all settings type lists on first load.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        base.IsLoading = true;
        await base.OnInitializedAsync();
        _primaryColour = await GetColour("--color-primary");
        _secondaryColour = await GetColour("--color-secondary");
        await LoadColours();
        await LoadAllTypes();
        base.IsLoading = false;
    }

    /// <summary>
    /// Loads all settings type lists (timesheet, contact, job, file, etc.) in parallel from the API.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
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

    /// <summary>
    /// Loads the timesheet type list from the API.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadTimesheetTypes()
    {
        Result<TimeTypeDto[]> res = await _apiService.GetTimeSheetTypes();
        if (res.IsSuccess && res.Value is { } v) _timesheetTypes = v.ToList();
    }

    /// <summary>
    /// Loads the contact type list from the API.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadContactTypes()
    {
        Result<ContactTypeDto[]> res = await _apiService.GetContactTypes();
        if (res.IsSuccess && res.Value is { } v) _contactTypes = v.ToList();
    }

    /// <summary>
    /// Loads the job type list from the API.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadJobTypes()
    {
        Result<JobTypeDto[]> res = await _apiService.GetJobTypes();
        if (res.IsSuccess && res.Value is { } v) _jobTypes = v.ToList();
    }

    /// <summary>
    /// Loads the job colour list from the API.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadJobColours()
    {
        Result<JobColourDto[]> res = await _apiService.GetJobColours();
        if (res.IsSuccess && res.Value is { } v) _jobColours = v.ToList();
    }

    /// <summary>
    /// Loads the file type list from the API.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadFileTypes()
    {
        Result<FileTypeDto[]> res = await _apiService.GetFileTypes();
        if (res.IsSuccess && res.Value is { } v) _fileTypes = v.ToList();
    }

    /// <summary>
    /// Loads the job task type list from the API.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadJobTaskTypes()
    {
        Result<JobTaskTypeDto[]> res = await _apiService.GetJobTaskTypes();
        if (res.IsSuccess && res.Value is { } v) _jobTaskTypes = v.ToList();
    }

    /// <summary>
    /// Loads the technical contact type list from the API.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadTechnicalContactTypes()
    {
        Result<TechnicalContactTypeDto[]> res = await _apiService.GetTechnicalContactTypes();
        if (res.IsSuccess && res.Value is { } v) _technicalContactTypes = v.ToList();
    }

    /// <summary>
    /// Loads the state/region list from the API.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadStates()
    {
        Result<StateDto[]> res = await _apiService.GetStates();
        if (res.IsSuccess && res.Value is { } v) _states = v.ToList();
    }

    /// <summary>
    /// Opens the edit-type dialog for the given type name and item, then reloads all types if the user saves.
    /// </summary>
    /// <param name="typeName">The type name (e.g. for API and dialog routing).</param>
    /// <param name="title">The dialog title.</param>
    /// <param name="item">The item to edit, or null for a new item.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
        if (colour == _primaryColour && primary || colour == _secondaryColour && !primary)
            return;

        if (primary)
            _primaryColour = colour;
        else
            _secondaryColour = colour;

        await Helpers.SetColour(_jsRuntime, colour, primary);

        SystemSettingsDto settingsObject = new()
        {
            PrimaryColour = _primaryColour,
            SecondaryColour = _secondaryColour
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