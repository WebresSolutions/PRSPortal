using System.Text.Json.Serialization;

namespace Portal.Shared.DTO.Integration;

/// <summary>
/// Response from GET /api/integration/xero/status indicating whether Xero is connected.
/// </summary>
public class XeroStatusResponse
{
    /// <summary>
    /// Whether the application has a stored Xero refresh token (is connected).
    /// </summary>
    [JsonPropertyName("connected")]
    public bool Connected { get; set; }
}
