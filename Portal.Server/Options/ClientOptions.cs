namespace Portal.Server.Options;

/// <summary>
/// Configuration options for client application settings
/// Contains authentication and redirect URI configuration
/// </summary>
public class ClientOptions
{
    /// <summary>
    /// Gets or sets the client identifier for authentication
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the display name of the client application
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the HTTPS post-logout redirect URI
    /// </summary>
    public string PostLogoutRedirectUri { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the HTTP post-logout redirect URI
    /// </summary>
    public string PostLogoutRedirectUriHttp { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the HTTPS redirect URI for authentication
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the HTTP redirect URI for authentication
    /// </summary>
    public string RedirectUriHttp { get; set; } = string.Empty;
}