using MudBlazor;
using Portal.Client.Components.Settings;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Integration;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Setting;
using Portal.Shared.DTO.Types;
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
    private List<ServiceTypeDto> _serviceTypes = [];
    private List<JobTypeStatusDto> _jobStatuses = [];

    private bool _xeroStatusLoaded;
    private bool _xeroConnected;
    private bool _xeroBusy;

    /// <summary>
    /// Loads theme colours, schedule colours, and all settings type lists on first load.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        base.IsLoading = true;
        _breadCrumbService.SetBreadCrumbItems(
          [
            new("Settings", href: "/settings", disabled: true),
          ]);
        await base.OnInitializedAsync();
        _primaryColour = await GetColour("--color-primary");
        _secondaryColour = await GetColour("--color-secondary");
        await LoadSettingsTypesBundle();
        await LoadXeroStatus();
        base.IsLoading = false;
    }

    /// <summary>
    /// Loads schedule colours and all settings type lists in one API call.
    /// </summary>
    private async Task LoadSettingsTypesBundle()
    {
        Result<AllSettingsTypesDto> res = await _apiService.GetAllSettingsTypes();
        if (res.IsSuccess && res.Value is { } bundle)
            ApplySettingsTypesBundle(bundle);
        else
            _snackbar.Add(res.ErrorDescription ?? "Failed to load settings lists", Severity.Error);
    }

    private void ApplySettingsTypesBundle(AllSettingsTypesDto bundle)
    {
        _timesheetTypes = bundle.TimesheetTypes.ToList();
        _contactTypes = bundle.ContactTypes.ToList();
        _jobTypes = bundle.JobTypes.ToList();
        _jobColours = bundle.JobColours.ToList();
        _fileTypes = bundle.FileTypes.ToList();
        _jobTaskTypes = bundle.JobTaskTypes.ToList();
        _technicalContactTypes = bundle.TechnicalContactTypes.ToList();
        _states = bundle.States.ToList();
        _colours = bundle.ScheduleColours.ToList();
        _serviceTypes = bundle.ServiceTypes.ToList();
        _jobStatuses = (bundle.JobStatuses ?? []).ToList();
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
        DialogParameters parameters = new()
        {
            [nameof(EditTypeDialog.TypeName)] = typeName,
            [nameof(EditTypeDialog.Title)] = title,
            [nameof(EditTypeDialog.Item)] = item,
            [nameof(EditTypeDialog.ApiService)] = _apiService,
            [nameof(EditTypeDialog.Snackbar)] = _snackbar
        };
        DialogOptions options = new() { CloseOnEscapeKey = true, NoHeader = true };
        IDialogReference dialog = await _dialog.ShowAsync<EditTypeDialog>(title, parameters, options);
        DialogResult? result = await dialog.Result;
        if (result is { Canceled: false })
            await LoadSettingsTypesBundle();
    }

    /// <summary>
    /// Opens the dialog to edit job pipeline statuses (bulk save for all job types).
    /// </summary>
    private async Task OpenEditJobStatusesDialog()
    {
        DialogParameters parameters = new()
        {
            [nameof(EditJobStatusesDialog.JobTypes)] = _jobTypes.ToList(),
            [nameof(EditJobStatusesDialog.Statuses)] = _jobStatuses.ToList(),
            [nameof(EditJobStatusesDialog.ApiService)] = _apiService,
            [nameof(EditJobStatusesDialog.Snackbar)] = _snackbar
        };
        DialogOptions options = new() { CloseOnEscapeKey = true, NoHeader = true, MaxWidth = MaxWidth.ExtraLarge, FullWidth = true };
        IDialogReference dialog = await _dialog.ShowAsync<EditJobStatusesDialog>("Job statuses", parameters, options);
        DialogResult? result = await dialog.Result;
        if (result is { Canceled: false })
            await LoadSettingsTypesBundle();
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
            if (colours.IsSuccess && colours.Value is { } list)
                _colours = list;
            else
                _snackbar.Add(colours.ErrorDescription ?? "Error occurred while refreshing schedule colours", Severity.Error);
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

    /// <summary>
    /// Loads the current Xero connection status from the API.
    /// </summary>
    private async Task LoadXeroStatus()
    {
        Result<XeroStatusResponse> res = await _apiService.GetXeroStatus();
        if (res.IsSuccess && res.Value is { } v)
        {
            _xeroConnected = v.Connected;
        }
        _xeroStatusLoaded = true;
    }

    /// <summary>
    /// Gets the Xero OAuth authorize URL and redirects the user to it to connect their Xero account.
    /// </summary>
    private async Task ConnectXero()
    {
        _xeroBusy = true;
        try
        {
            Result<XeroAuthorizeResponse> res = await _apiService.GetXeroAuthorizeUrl();
            if (res.IsSuccess && !string.IsNullOrWhiteSpace(res.Value?.Url))
            {
                _navigationManager.NavigateTo(res.Value!.Url, forceLoad: true);
                return;
            }
            _snackbar.Add(res.ErrorDescription ?? "Failed to get Xero authorization URL", Severity.Error);
        }
        finally
        {
            _xeroBusy = false;
        }
    }

    /// <summary>
    /// Disconnects the Xero integration (removes stored tokens) and refreshes status.
    /// </summary>
    private async Task DisconnectXero()
    {
        _xeroBusy = true;
        try
        {
            Result<bool> res = await _apiService.DisconnectXero();
            if (res.IsSuccess)
            {
                _xeroConnected = false;
                _snackbar.Add("Xero disconnected.", Severity.Success);
            }
            else
            {
                _snackbar.Add(res.ErrorDescription ?? "Failed to disconnect Xero", Severity.Error);
            }
        }
        finally
        {
            _xeroBusy = false;
        }
    }
}