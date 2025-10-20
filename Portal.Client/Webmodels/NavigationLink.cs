namespace Portal.Client.Webmodels;

public class NavigationLink()
{

    /// <summary>
    /// The display name of the link.
    /// </summary>
    public string Link { get; set; } = string.Empty;

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
