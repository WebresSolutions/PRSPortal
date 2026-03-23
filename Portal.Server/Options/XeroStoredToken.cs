namespace Portal.Server.Services.Instances;

public partial class XeroIntegrationService
{
    private sealed class XeroStoredToken
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime AccessTokenExpiresAtUtc { get; set; }
        public string? TenantId { get; set; }
    }
}
