namespace Portal.Client.Webmodels;

public class NavLinkItem
{
    public string Link { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool MatchAll { get; set; } = false;
}
