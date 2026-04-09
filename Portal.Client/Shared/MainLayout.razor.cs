using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
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
    private bool _mobileNavOpen;
    private readonly HashSet<string> _mobileExpandedGroups = [];

    private List<NavLinkItem> navLinks = [];
    private List<BreadcrumbItem> _breadcrumbItems = [];

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _navigationManager.LocationChanged += OnLocationChanged;
        string today = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
        navLinks =
        [   new NavLinkItem { Link = "", Title = "Home", Icon = Icons.Material.Filled.Home, MatchAll = true },
            new NavLinkItem { Link = "jobs", Title = "Jobs", Icon = Icons.Material.Filled.Build },
            new NavLinkItem { Link = "contacts", Title = "Contacts", Icon = Icons.Material.Filled.AccountBox },
            new NavLinkItem { Link = "councils", Title = "Councils", Icon = Icons.Material.Filled.Group },
            new NavLinkItem { Link = "quotes", Title = "Quotes", Icon = Icons.Material.Filled.Task },
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
        RefreshBreadcrumbs();
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e) =>
        _ = InvokeAsync(() =>
        {
            RefreshBreadcrumbs();
            StateHasChanged();
        });

    private void RefreshBreadcrumbs()
    {
        string relative = _navigationManager.ToBaseRelativePath(_navigationManager.Uri).Split('?')[0].TrimEnd('/');
        string[] raw = relative.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string[] lower = [.. raw.Select(s => s.ToLowerInvariant())];

        if (raw.Length == 0)
        {
            _breadcrumbItems = [new BreadcrumbItem("Home", href: null, disabled: true)];
            return;
        }

        List<BreadcrumbItem> items = [new BreadcrumbItem("Home", "/")];

        switch (lower[0])
        {
            case "jobs":
                AppendEntitySection(items, raw, lower, "Jobs", "job", "Job");
                break;
            case "contacts":
                AppendEntitySection(items, raw, lower, "Contacts", "contact", "Contact");
                break;
            case "councils":
                AppendEntitySection(items, raw, lower, "Councils", "council", "Council");
                break;
            case "quotes":
                AppendQuotesSection(items, raw, lower);
                break;
            case "timesheets":
                AppendTimesheetsSection(items, raw, lower);
                break;
            case "schedule":
                AppendScheduleSection(items, raw, lower);
                break;
            case "week":
                AppendWeekSection(items, raw, lower);
                break;
            case "authentication":
                AppendAuthenticationSection(items, raw, lower);
                break;
            case "settings":
            case "admin":
            case "notes":
            case "tasks":
            case "tools":
            case "invoices":
            case "counter":
                AppendSimpleSection(items, raw, lower);
                break;
            default:
                AppendGenericTrail(items, raw);
                break;
        }

        _breadcrumbItems = items;
    }

    private static void AppendEntitySection(List<BreadcrumbItem> items, string[] raw, string[] lower, string listTitle, string createEditNoun, string detailNoun)
    {
        string rootHref = "/" + raw[0];

        if (lower.Length == 1)
        {
            items.Add(new BreadcrumbItem(listTitle, href: null, disabled: true));
            return;
        }

        items.Add(new BreadcrumbItem(listTitle, rootHref));

        if (lower.Length == 2 && lower[1] == "create")
        {
            items.Add(new BreadcrumbItem($"Create {createEditNoun}", href: null, disabled: true));
            return;
        }

        if (lower.Length == 3 && lower[1] == "edit" && int.TryParse(raw[2], out _))
        {
            items.Add(new BreadcrumbItem($"Edit {createEditNoun}", href: null, disabled: true));
            return;
        }

        if (lower.Length == 2 && int.TryParse(raw[1], out _))
        {
            items.Add(new BreadcrumbItem($"{detailNoun} {raw[1]}", href: null, disabled: true));
            return;
        }

        AppendGenericTrailFrom(items, raw, 1);
    }

    private static void AppendQuotesSection(List<BreadcrumbItem> items, string[] raw, string[] lower)
    {
        string rootHref = "/" + raw[0];

        if (lower.Length == 1)
        {
            items.Add(new BreadcrumbItem("Quotes", href: null, disabled: true));
            return;
        }

        items.Add(new BreadcrumbItem("Quotes", rootHref));

        if (lower.Length == 2 && lower[1] == "create")
        {
            items.Add(new BreadcrumbItem("Create quote", href: null, disabled: true));
            return;
        }

        if (lower.Length == 2 && int.TryParse(raw[1], out _))
        {
            items.Add(new BreadcrumbItem($"Quote {raw[1]}", href: null, disabled: true));
            return;
        }

        AppendGenericTrailFrom(items, raw, 1);
    }

    private static void AppendTimesheetsSection(List<BreadcrumbItem> items, string[] raw, string[] lower)
    {
        string today = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");

        if (lower.Length == 1)
        {
            items.Add(new BreadcrumbItem("Timesheets", href: null, disabled: true));
            return;
        }

        items.Add(new BreadcrumbItem("Times", $"/{raw[0]}/{today}"));

        if (lower.Length == 2)
        {
            items.Add(new BreadcrumbItem(FormatDateLabel(raw[1]), href: null, disabled: true));
            return;
        }

        AppendGenericTrailFrom(items, raw, 1);
    }

    private static void AppendScheduleSection(List<BreadcrumbItem> items, string[] raw, string[] lower)
    {
        string rootHref = "/" + raw[0];

        if (lower.Length == 1)
        {
            items.Add(new BreadcrumbItem("Daily", href: null, disabled: true));
            return;
        }

        items.Add(new BreadcrumbItem("Daily", rootHref));

        if (lower.Length >= 2)
        {
            items.Add(new BreadcrumbItem(FormatDateLabel(raw[1]), href: null, disabled: true));
        }

        if (lower.Length >= 3)
        {
            items.Add(new BreadcrumbItem(JobTypeLabel(lower[2]), href: null, disabled: true));
        }
    }

    private static void AppendWeekSection(List<BreadcrumbItem> items, string[] raw, string[] lower)
    {
        string rootHref = "/" + raw[0];

        if (lower.Length == 1)
        {
            items.Add(new BreadcrumbItem("Weekly", href: null, disabled: true));
            return;
        }

        items.Add(new BreadcrumbItem("Weekly", rootHref));

        if (lower.Length >= 2)
        {
            items.Add(new BreadcrumbItem(FormatDateLabel(raw[1]), href: null, disabled: true));
        }

        if (lower.Length >= 3)
        {
            items.Add(new BreadcrumbItem(JobTypeLabel(lower[2]), href: null, disabled: true));
        }
    }

    private static void AppendAuthenticationSection(List<BreadcrumbItem> items, string[] raw, string[] lower)
    {
        if (lower.Length >= 2)
        {
            items.Add(new BreadcrumbItem(HumanizeSegment(lower[1]), href: null, disabled: true));
            return;
        }

        items.Add(new BreadcrumbItem("Account", href: null, disabled: true));
    }

    private static void AppendSimpleSection(List<BreadcrumbItem> items, string[] raw, string[] lower)
    {
        if (lower.Length == 1)
        {
            items.Add(new BreadcrumbItem(HumanizeSegment(lower[0]), href: null, disabled: true));
            return;
        }

        AppendGenericTrailFrom(items, raw, 0);
    }

    private static void AppendGenericTrail(List<BreadcrumbItem> items, string[] raw) => AppendGenericTrailFrom(items, raw, 0);

    private static void AppendGenericTrailFrom(List<BreadcrumbItem> items, string[] raw, int startIndex)
    {
        for (int i = startIndex; i < raw.Length; i++)
        {
            string path = "/" + string.Join("/", raw.Take(i + 1));
            bool last = i == raw.Length - 1;
            string text = HumanizeSegment(raw[i]);
            if (last)
                items.Add(new BreadcrumbItem(text, href: null, disabled: true));
            else
                items.Add(new BreadcrumbItem(text, path));
        }
    }

    private static string FormatDateLabel(string segment) =>
        DateOnly.TryParse(segment, out DateOnly d) ? d.ToString("MMM d, yyyy") : HumanizeSegment(segment);

    private static string JobTypeLabel(string segment)
    {
        if (int.TryParse(segment, out int t))
        {
            return t switch
            {
                1 => "Construction",
                2 => "Surveying",
                _ => HumanizeSegment(segment)
            };
        }

        return HumanizeSegment(segment);
    }

    private static string HumanizeSegment(string segment)
    {
        if (string.IsNullOrEmpty(segment))
            return segment;

        string[] parts = segment.Split('-', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts.Select(static p => char.ToUpperInvariant(p[0]) + p[1..].ToLowerInvariant()));
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
    private void BeginSignOut(MouseEventArgs args)
    {
        _navigationManager.NavigateToLogout("authentication/logout");
    }

    private string MainClass => _collapsed ? "main full" : "main";
    private bool MobileNavOpen => _mobileNavOpen;

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

    private Task ToggleMobileNav()
    {
        _mobileNavOpen = !_mobileNavOpen;
        return Task.CompletedTask;
    }

    private Task CloseMobileNav()
    {
        _mobileNavOpen = false;
        return Task.CompletedTask;
    }

    private void ToggleMobileGroup(NavLinkItem navItem)
    {
        string key = GetMobileGroupKey(navItem);
        if (!_mobileExpandedGroups.Add(key))
            _mobileExpandedGroups.Remove(key);
    }

    private bool IsMobileGroupOpen(NavLinkItem navItem) => _mobileExpandedGroups.Contains(GetMobileGroupKey(navItem));

    private static string GetMobileGroupKey(NavLinkItem navItem) => $"{navItem.Title}:{navItem.Link}";

    public ValueTask DisposeAsync()
    {
        _navigationManager.LocationChanged -= OnLocationChanged;
        GC.SuppressFinalize(this);
        _ = _hotKeysContext?.DisposeAsync();
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