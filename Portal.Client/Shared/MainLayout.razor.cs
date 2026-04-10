using Microsoft.JSInterop;
using MudBlazor;
using Portal.Client.Components;
using Portal.Client.Webmodels;
using Portal.Shared.DTO.Setting;
using Portal.Shared.ResponseModels;
using System.Text.Json;
using Toolbelt.Blazor.HotKeys2;

namespace Portal.Client.Shared;

public partial class MainLayout : IAsyncDisposable
{
    protected HotKeysContext? _hotKeysContext;
    private bool _collapsed = true;
    private bool headerVisible = true;
    private List<NavLinkItem> navLinks = [];
    private List<BreadcrumbItem> _breadcrumbItems = [];

    protected override void OnInitialized()
    {
        base.OnInitialized();
        string today = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
        navLinks =
        [   new NavLinkItem { Link = "", Title = "Home", Icon = Icons.Material.Filled.Home, MatchAll = true },
            new NavLinkItem { Link = "quotes", Title = "Quotes", Icon = Icons.Material.Filled.Task },
            new NavLinkItem { Link = "jobs", Title = "Jobs", Icon = Icons.Material.Filled.Build },
            new NavLinkItem { Link = "contacts", Title = "Contacts", Icon = Icons.Material.Filled.AccountBox },
            new NavLinkItem { Link = "councils", Title = "Councils", Icon = Icons.Material.Filled.Group },
            new NavLinkItem { Link = $"timeSheets/{today}", Title = "Times", Icon = Icons.Material.Filled.Timelapse },
            new NavLinkItem { Link = $"schedule", Title = "Daily", Icon = Icons.Material.Outlined.Today,
                SubLinks = [
                        new NavLinkItem { Link = $"schedule/{today}/1", Title = "Construction", Icon = Icons.Material.Filled.CalendarMonth },
                        new NavLinkItem { Link = $"schedule/{today}/2", Title = "Surveying", Icon = Icons.Material.Filled.CalendarMonth }
                    ],
            },
            new NavLinkItem { Link = $"week", Title = "Weekly", Icon = Icons.Material.Filled.CalendarMonth,
                SubLinks = [
                    new NavLinkItem { Link = $"week/{today}/1", Title = "Construction", Icon = Icons.Material.Filled.CalendarMonth },
                    new NavLinkItem { Link = $"week/{today}/2", Title = "Surveying", Icon = Icons.Material.Filled.CalendarMonth }
                ],
            },
            new NavLinkItem { Link = "Settings", Title = "Settings", Icon = Icons.Material.Filled.Settings },
            new NavLinkItem { Link = "Admin", Title = "Admin", Icon = Icons.Material.Filled.AdminPanelSettings }
        ];
        _breadCrumbService.Subscribe(StateHasChanged);
        _breadCrumbService.SetBreadCrumbItems(
          [
            new("Home", href: "/", disabled: false)
          ]);
    }

    private async Task ToggleNav()
    {
        _collapsed = !_collapsed;
        SystemSettingsDto? sessionSettings = _sessionStorage.GetItem<SystemSettingsDto>("SystemSettings");

        if (sessionSettings is null)
        {
            Result<SystemSettingDto> appsettings = await _apiService.GetSystemSettings();
            if (appsettings.IsSuccess && appsettings.Value is not null)
                sessionSettings = JsonSerializer.Deserialize<SystemSettingsDto>(appsettings.Value.SettingJson);
            else
                return;
        }

        sessionSettings?.NavbarCollapsed = _collapsed;
        // Save to session storage for next time
        _sessionStorage.SetItem("SystemSettings", sessionSettings);
        string settingsJson = JsonSerializer.Serialize(sessionSettings);
        SystemSettingDto obj = new() { SettingJson = settingsJson };
        _ = await _apiService.UpdateSystemSettings(obj);
    }
    private string MainClass => _collapsed ? "main full" : "main";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _hotKeysContext = _hotKeys.CreateContext()
                .Add(ModCode.Ctrl | ModCode.Shift, Code.ArrowUp, () => GoToNextNavItem(true), new HotKeyOptions() { Description = "Navigate to the next page up" })
                .Add(ModCode.Ctrl | ModCode.Shift, Code.ArrowDown, () => GoToNextNavItem(false), new HotKeyOptions() { Description = "Navigate to the next page down" })
                .Add(ModCode.Ctrl | ModCode.Shift, Code.S, OpenDialogAsync, new HotKeyOptions() { Description = "Opens the quick search for jobs" });
            await _jsRuntime.InvokeVoidAsync("initializeScrollHeader");
            await LoadSystemSettings();
        }
    }

    private async Task OpenDialogAsync() => await _dialog.ShowAsync<SearchDialog>("", new DialogOptions() { CloseButton = true, CloseOnEscapeKey = true });

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        _ = _hotKeysContext?.DisposeAsync();
        _breadCrumbService.Dispose(StateHasChanged);
        return ValueTask.CompletedTask;
    }

    private void GoToNextNavItem(bool up)
    {
        // find the current nav item. Remove any query strings and trailing slashes
        string uri = _navigationManager.ToBaseRelativePath(_navigationManager.Uri).TrimEnd('/').Split('?')[0];
        if (uri.Contains('/'))
            uri = uri.Split('/')[0];

        // Find the index of the current nav item
        int currentIndex = navLinks?.FindIndex(n => n.Link.Equals(uri, StringComparison.OrdinalIgnoreCase)) ?? -1;

        if (up)
            currentIndex--;
        else
            currentIndex++;

        // Loop around: wrap to end if going before start, wrap to start if going past end
        if (currentIndex < 0)
            currentIndex = navLinks!.Count - 1;
        else if (currentIndex >= navLinks!.Count)
            currentIndex = 0;

        NavLinkItem newNavItem = navLinks[currentIndex];

        if (newNavItem.DoAction is not null)
            newNavItem.DoAction.Invoke();
        else if (newNavItem.Link is not null)
            _navigationManager.NavigateTo(newNavItem.Link);

    }


    private async Task LoadSystemSettings()
    {
        // Try to load from session storage first (faster, no API call)
        SystemSettingsDto? sessionSettings = _sessionStorage.GetItem<SystemSettingsDto>("SystemSettings");

        if (sessionSettings is not null &&
            !string.IsNullOrWhiteSpace(sessionSettings.PrimaryColour) &&
            !string.IsNullOrWhiteSpace(sessionSettings.SecondaryColour))
        {
            await Helpers.SetColour(_jsRuntime, sessionSettings.PrimaryColour, true);
            await Helpers.SetColour(_jsRuntime, sessionSettings.SecondaryColour, false);
            return;
        }

        // Fallback to API if session storage is empty or invalid
        Result<SystemSettingDto> appsettings = await _apiService.GetSystemSettings();

        if (appsettings.Error is not null || appsettings.Value is null)
            return;

        if (string.IsNullOrWhiteSpace(appsettings.Value.SettingJson))
            return;

        try
        {
            SystemSettingsDto? settings = JsonSerializer.Deserialize<SystemSettingsDto>(appsettings.Value.SettingJson);
            if (settings is null ||
                string.IsNullOrWhiteSpace(settings.PrimaryColour) ||
                string.IsNullOrWhiteSpace(settings.SecondaryColour))
                return;

            _collapsed = settings.NavbarCollapsed;
            // Save to session storage for next time
            _sessionStorage.SetItem("SystemSettings", settings);

            // Apply the colors
            await Helpers.SetColour(_jsRuntime, settings.PrimaryColour, true);
            await Helpers.SetColour(_jsRuntime, settings.SecondaryColour, false);
        }
        catch (JsonException)
        {
            // Invalid JSON, skip setting colors
        }
    }
}