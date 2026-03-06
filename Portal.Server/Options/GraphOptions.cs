namespace Portal.Server.Options;

/// <summary>
/// Contains the config settings for the Igraph service
/// </summary>
public class GraphOptions
{
    /// <summary>
    /// The Client Id
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>
    /// The Tenant Id
    /// </summary>
    public string TenantId { get; set; } = string.Empty;
    /// <summary>
    /// The Client Secret
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
    /// <summary>
    ///  The Site Id
    /// </summary>
    public string SiteId { get; set; } = string.Empty;
    /// <summary>
    /// The Drive Id
    /// </summary>
    public string DriveId { get; set; } = string.Empty;
    /// <summary>
    /// The scopes for the graph client
    /// </summary>
    public string Scopes { get; set; } = string.Empty;
    /// <summary>
    /// The file url key
    /// </summary>
    public string FileUrlKey { get; set; } = string.Empty;
    /// <summary>
    /// The base folder
    /// </summary>
    public string BaseFolder { get; set; } = string.Empty;
    /// <summary>
    /// The cache folder
    /// </summary>
    public string CacheFolder { get; set; } = string.Empty;
}
