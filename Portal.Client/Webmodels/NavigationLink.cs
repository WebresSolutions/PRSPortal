namespace Portal.Client.Webmodels;

/// <summary>
/// Represents a navigation link with optional sub-navigation items
/// Supports hierarchical navigation structures
/// </summary>
public class NavigationLink()
{

    /// <summary>
    /// Gets or sets the URL path for the navigation link
    /// </summary>
    public string Link { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display title for the navigation link
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The icons
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Navigation links for sub menus.
    /// </summary>
    public NavigationLink[] SubLinks { get; set; } = [];
}
