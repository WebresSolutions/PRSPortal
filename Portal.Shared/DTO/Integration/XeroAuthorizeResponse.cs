using System.Text.Json.Serialization;

namespace Portal.Shared.DTO.Integration;

/// <summary>
/// Response from GET /api/integration/xero/authorize containing the Xero OAuth authorization URL.
/// </summary>
public class XeroAuthorizeResponse
{
    /// <summary>
    /// The URL to redirect the user to for Xero OAuth authorization.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
