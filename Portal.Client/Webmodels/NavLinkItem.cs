namespace Portal.Client.Webmodels;

/// <summary>
/// Represents a navigation link item in the application menu
/// Contains information for rendering navigation elements
/// </summary>
public class NavLinkItem
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
    /// Gets or sets the icon identifier or class name for the navigation link
    /// </summary>
    public string Icon { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether the link should match all sub-paths
    /// When true, the link will be active for all paths starting with the Link value
    /// </summary>
    public bool MatchAll { get; set; } = false;
}
