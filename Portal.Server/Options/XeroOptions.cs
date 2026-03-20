namespace Portal.Server.Options;

public class XeroOptions
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string Scope { get; set; }
    public required string CallbackUri { get; set; }
    public required int TokenExpiryDays { get; set; }
    /// <summary>Xero tenant (org) ID; can be left empty and will be set from OAuth connection.</summary>
    public string TenantId { get; set; } = string.Empty;
    /// <summary>Where to redirect the user after successful OAuth callback (e.g. https://yourapp.com/settings?xero=connected).</summary>
    public string? FrontendRedirectUri { get; set; }
}
