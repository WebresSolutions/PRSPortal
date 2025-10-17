namespace Portal.Server.Options;

public class ClientOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PostLogoutRedirectUri { get; set; } = string.Empty;
    public string PostLogoutRedirectUriHttp { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string RedirectUriHttp { get; set; } = string.Empty;
}