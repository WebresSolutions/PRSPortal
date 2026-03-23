namespace Portal.Server.Services.Instances;

public partial class XeroIntegrationService
{
    private sealed class TokenResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
