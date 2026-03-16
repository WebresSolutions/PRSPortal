

using Microsoft.Extensions.Options;
using Portal.Data;
using Portal.Server.Options;
using Portal.Server.Services.Interfaces;

namespace Portal.Server.Services.Instances;

public class XeroIntegrationService(PrsDbContext db, IOptions<XeroOptions> options, IHttpClientFactory httpClientFactory) : IXeroIntegrationService
{
    private const string TokenEndpoint = "https://identity.xero.com/connect/token";
    private const string ConnectionsEndpoint = "https://api.xero.com/connections";
    private static readonly TimeSpan AccessTokenRefreshBuffer = TimeSpan.FromMinutes(2); // Refresh 2 min before expiry
    private readonly XeroOptions _options = options.Value;

    //public async Task<XeroConnectionResult?> GetOrRefreshConnectionAsync(int userId, CancellationToken cancellationToken = default)
    //{
    //    // Use static token if configured (dev)
    //    if (!string.IsNullOrEmpty(_options.AccessToken) && !string.IsNullOrEmpty(_options.TenantId))
    //        return new XeroConnectionResult(_options.AccessToken, _options.TenantId);

    //    //XeroConnection? conn = await _db.XeroConnections.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    //    XeroConnection? conn = null;
    //    if (conn == null)
    //        return null;

    //    // Refresh if expired or within buffer (Xero: access token 30 min)
    //    if (conn.ExpiresAtUtc <= DateTime.UtcNow.Add(AccessTokenRefreshBuffer))
    //    {
    //        (string? newAccess, string? newRefresh, int expiresIn) = await RefreshTokensAsync(conn.RefreshToken, cancellationToken);
    //        if (newAccess == null)
    //            return null; // Refresh failed; user may need to re-connect

    //        conn.AccessToken = newAccess;
    //        if (!string.IsNullOrEmpty(newRefresh))
    //            conn.RefreshToken = newRefresh;
    //        conn.ExpiresAtUtc = DateTime.UtcNow.AddSeconds(expiresIn);
    //        conn.ModifiedOn = DateTime.UtcNow;
    //        await db.SaveChangesAsync(cancellationToken);
    //    }

    //    return new XeroConnectionResult(conn.AccessToken, conn.TenantId);
    //}

    //public async Task<bool> HasConnectionAsync(int userId, CancellationToken cancellationToken = default)
    //{
    //    if (!string.IsNullOrEmpty(_options.AccessToken))
    //        return true;
    //    //return await _db.XeroConnections.AnyAsync(c => c.UserId == userId, cancellationToken);
    //    return true;
    //}

    //public async Task<(bool Success, string? Error)> SaveConnectionFromCodeAsync(int userId, string code, CancellationToken cancellationToken = default)
    //{
    //    (string? accessToken, string? refreshToken, int expiresIn) = await ExchangeCodeForTokensAsync(code, cancellationToken);
    //    if (accessToken == null)
    //        return (false, "Failed to exchange code for tokens.");

    //    HttpClient client = httpClientFactory.CreateClient();
    //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    //    HttpResponseMessage connResponse = await client.GetAsync(ConnectionsEndpoint, cancellationToken);
    //    if (!connResponse.IsSuccessStatusCode)
    //        return (false, "Failed to get Xero connections.");

    //    string json = await connResponse.Content.ReadAsStringAsync(cancellationToken);
    //    List<XeroConnectionDto>? connections = JsonSerializer.Deserialize<List<XeroConnectionDto>>(json);
    //    XeroConnectionDto? org = connections?.FirstOrDefault(c => c.TenantType == "ORGANISATION");
    //    if (org == null)
    //        return (false, "No Xero organisation selected.");

    //    XeroConnection? existing = null;
    //    //XeroConnection? existing = await _db.XeroConnections.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    //    if (existing != null)
    //    {
    //        existing.AccessToken = accessToken;
    //        existing.RefreshToken = refreshToken ?? existing.RefreshToken;
    //        existing.ExpiresAtUtc = DateTime.UtcNow.AddSeconds(expiresIn);
    //        existing.TenantId = org.TenantId;
    //        existing.TenantName = org.TenantName;
    //        existing.ModifiedOn = DateTime.UtcNow;
    //    }
    //    else
    //    {
    //        //_db.XeroConnections.Add(new XeroConnection
    //        //{
    //        //    UserId = userId,
    //        //    AccessToken = accessToken,
    //        //    RefreshToken = refreshToken ?? "",
    //        //    ExpiresAtUtc = DateTime.UtcNow.AddSeconds(expiresIn),
    //        //    TenantId = org.TenantId,
    //        //    TenantName = org.TenantName
    //        //});
    //    }
    //    await db.SaveChangesAsync(cancellationToken);
    //    return (true, null);
    //}

    //public async Task DisconnectAsync(int userId, CancellationToken cancellationToken = default)
    //{
    //    XeroConnection? conn = null;
    //    //XeroConnection? conn = await _db.XeroConnections.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    //    if (conn != null)
    //    {
    //        //_db.XeroConnections.Remove(conn);
    //        await db.SaveChangesAsync(cancellationToken);
    //    }
    //}

    //private async Task<(string? AccessToken, string? RefreshToken, int ExpiresIn)> ExchangeCodeForTokensAsync(string code, CancellationToken cancellationToken)
    //{
    //    HttpClient client = httpClientFactory.CreateClient();
    //    string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
    //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
    //    Dictionary<string, string> form = new()
    //    {
    //        ["grant_type"] = "authorization_code",
    //        ["code"] = code,
    //        ["redirect_uri"] = _options.RedirectUri
    //    };
    //    FormUrlEncodedContent content = new(form);
    //    HttpResponseMessage response = await client.PostAsync(TokenEndpoint, content, cancellationToken);
    //    if (!response.IsSuccessStatusCode)
    //        return (null, null, 0);
    //    string json = await response.Content.ReadAsStringAsync(cancellationToken);
    //    XeroTokenResponse? token = JsonSerializer.Deserialize<XeroTokenResponse>(json);
    //    return token != null ? (token.AccessToken, token.RefreshToken, token.ExpiresIn) : (null, null, 0);
    //}

    //private async Task<(string? AccessToken, string? RefreshToken, int ExpiresIn)> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken)
    //{
    //    HttpClient client = httpClientFactory.CreateClient();
    //    string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
    //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
    //    Dictionary<string, string> form = new()
    //    {
    //        ["grant_type"] = "refresh_token",
    //        ["refresh_token"] = refreshToken
    //    };
    //    FormUrlEncodedContent content = new(form);
    //    HttpResponseMessage response = await client.PostAsync(TokenEndpoint, content, cancellationToken);
    //    if (!response.IsSuccessStatusCode)
    //        return (null, null, 0);
    //    string json = await response.Content.ReadAsStringAsync(cancellationToken);
    //    XeroTokenResponse? token = JsonSerializer.Deserialize<XeroTokenResponse>(json);
    //    return token != null ? (token.AccessToken, token.RefreshToken, token.ExpiresIn) : (null, null, 0);
    //}
}
