using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Options;
using Portal.Server.Services.Interfaces;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Model.PayrollAu;

namespace Portal.Server.Services.Instances;

/// <summary>
/// Handles Xero OAuth2 connection: one-time admin sign-in, store refresh token, provide valid access tokens for the API.
/// </summary>
public partial class XeroIntegrationService(
    PrsDbContext db,
    IOptions<XeroOptions> options,
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IPayrollAuApi payrollAuApi) : IXeroIntegrationService
{
    private const string TokenEndpoint = "https://identity.xero.com/connect/token";
    private const string ConnectionsEndpoint = "https://api.xero.com/connections";
    private const string AuthorizeEndpoint = "https://login.xero.com/identity/connect/authorize";
    private const string StateCachePrefix = "Xero:State:";
    private static readonly TimeSpan StateValidFor = TimeSpan.FromMinutes(10);

    private readonly PrsDbContext _db = db;
    private readonly XeroOptions _options = options.Value;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IMemoryCache _cache = cache;
    private readonly IPayrollAuApi _payrollAuApi = payrollAuApi;

    /// <inheritdoc />
    public async Task<Dictionary<string, LeaveApplication>> GetLeaveApplications()
    {
        try
        {
            (string? accessToken, string? tenantId) = await GetValidAccessTokenAsync();
            Employees employees = await _payrollAuApi.GetEmployeesAsync(accessToken, tenantId);
            LeaveApplications leaves = await _payrollAuApi.GetLeaveApplicationsAsync(accessToken, tenantId);
            // Match Employee Details to the Users
            Dictionary<string, LeaveApplication> leaveAndEmployees = [];
            foreach (LeaveApplication? leave in leaves._LeaveApplications)
            {
                Employee? employee = employees._Employees.FirstOrDefault(x => x.EmployeeID == leave.EmployeeID);
                if (employee is null)
                    continue;

                leaveAndEmployees.Add(employee.Email, leave);
            }


            return leaveAndEmployees;
        }
        catch (Exception)
        {

            throw;
        }
    }


    /// <inheritdoc />
    public string GetAuthorizationUrl(string? state = null)
    {
        state ??= GenerateState();
        _cache.Set(StateCachePrefix + state, true, StateValidFor);

        Dictionary<string, string> query = new()
        {
            ["response_type"] = "code",
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = _options.CallbackUri,
            ["scope"] = _options.Scope,
            ["state"] = state
        };

        string queryString = string.Join("&", query.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{AuthorizeEndpoint}?{queryString}";
    }

    /// <inheritdoc />
    public async Task<bool> HandleCallbackAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        string? code = context.Request.Query["code"].FirstOrDefault();
        string? state = context.Request.Query["state"].FirstOrDefault();
        string? error = context.Request.Query["error"].FirstOrDefault();

        if (!string.IsNullOrEmpty(error))
        {
            RedirectToFrontend(context, success: false, error);
            return false;
        }

        if (string.IsNullOrEmpty(code))
        {
            RedirectToFrontend(context, success: false, "missing_code");
            return false;
        }

        if (string.IsNullOrEmpty(state) || !_cache.TryGetValue(StateCachePrefix + state, out _))
        {
            RedirectToFrontend(context, success: false, "invalid_state");
            return false;
        }
        _cache.Remove(StateCachePrefix + state);

        HttpClient httpClient = _httpClientFactory.CreateClient();
        TokenResponse? tokenResponse = await ExchangeCodeForTokenAsync(httpClient, code, cancellationToken);
        if (tokenResponse == null)
        {
            RedirectToFrontend(context, success: false, "token_exchange_failed");
            return false;
        }

        string? tenantId = await GetFirstTenantIdAsync(httpClient, tokenResponse.AccessToken, cancellationToken);
        if (string.IsNullOrEmpty(tenantId))
            tenantId = _options.TenantId;

        XeroStoredToken stored = new()
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            AccessTokenExpiresAtUtc = tokenResponse.ExpiresAtUtc,
            TenantId = tenantId
        };
        await SaveRefreshTokenAsync(stored, cancellationToken);

        RedirectToFrontend(context, success: true);
        return true;
    }

    /// <inheritdoc />
    public async Task<(string? AccessToken, string? TenantId)> GetValidAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        XeroStoredToken? stored = await LoadTokenAsync(cancellationToken);
        if (stored == null)
            return (null, null);

        if (DateTime.UtcNow >= stored.AccessTokenExpiresAtUtc.AddMinutes(-2))
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            TokenResponse? refreshed = await RefreshTokenAsync(httpClient, stored.RefreshToken, cancellationToken);

            if (refreshed == null)
                return (null, null);

            stored = new XeroStoredToken
            {
                AccessToken = refreshed.AccessToken,
                RefreshToken = refreshed.RefreshToken ?? stored.RefreshToken,
                AccessTokenExpiresAtUtc = refreshed.ExpiresAtUtc,
                TenantId = stored.TenantId
            };

            await SaveRefreshTokenAsync(stored, cancellationToken);
        }

        return (stored.AccessToken, stored.TenantId);
    }

    /// <inheritdoc />
    public async Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default)
    {
        XeroStoredToken? stored = await LoadTokenAsync(cancellationToken);
        return stored != null && !string.IsNullOrEmpty(stored.RefreshToken);
    }

    /// <inheritdoc />
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        XeroAccess? setting = await _db.XeroAccesses.FirstOrDefaultAsync(x => x.Expires > DateTime.UtcNow, cancellationToken);
        if (setting != null)
        {
            _db.XeroAccesses.Remove(setting);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Generates a random state the the url callback
    /// </summary>
    /// <returns></returns>
    private static string GenerateState()
    {
        byte[] bytes = new byte[24];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private async Task<TokenResponse?> ExchangeCodeForTokenAsync(HttpClient httpClient, string code, CancellationToken cancellationToken)
    {
        Dictionary<string, string> body = new()
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _options.CallbackUri,
            ["scope"] = _options.Scope
        };
        HttpRequestMessage request = new(HttpMethod.Post, TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(body)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}")));

        HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        string? accessToken = root.TryGetProperty("access_token", out JsonElement at) ? at.GetString() : null;
        string? refreshToken = root.TryGetProperty("refresh_token", out JsonElement rt) ? rt.GetString() : null;
        int expiresIn = root.TryGetProperty("expires_in", out JsonElement ex) ? ex.GetInt32() : 1800;
        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            return null;

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(expiresIn)
        };
    }

    private async Task<string?> GetFirstTenantIdAsync(HttpClient httpClient, string accessToken, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = new(HttpMethod.Get, ConnectionsEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement arr = doc.RootElement;
        if (arr.ValueKind != JsonValueKind.Array || arr.GetArrayLength() == 0)
            return null;
        JsonElement first = arr[0];
        return first.TryGetProperty("tenantId", out JsonElement id) ? id.GetString() : null;
    }

    private async Task<TokenResponse?> RefreshTokenAsync(HttpClient httpClient, string refreshToken, CancellationToken cancellationToken)
    {
        Dictionary<string, string> body = new()
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        };

        HttpRequestMessage request = new(HttpMethod.Post, TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(body)
        };

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}")));

        HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        string? accessToken = root.TryGetProperty("access_token", out JsonElement at) ? at.GetString() : null;
        string? newRefresh = root.TryGetProperty("refresh_token", out JsonElement rt) ? rt.GetString() : null;
        int expiresIn = root.TryGetProperty("expires_in", out JsonElement ex) ? ex.GetInt32() : 1800;
        if (string.IsNullOrEmpty(accessToken))
            return null;

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefresh ?? refreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(expiresIn)
        };
    }

    /// <summary>
    /// Redirects the to the front end
    /// </summary>
    /// <param name="context"></param>
    /// <param name="success"></param>
    /// <param name="error"></param>
    private void RedirectToFrontend(HttpContext context, bool success, string? error = null)
    {
        string baseUri = _options.FrontendRedirectUri ?? "/";
        string separator = baseUri.Contains('?') ? "&" : "?";
        string query = success ? "xero=connected" : (!string.IsNullOrEmpty(error) ? "xero_error=" + Uri.EscapeDataString(error) : "");
        string redirectUrl = string.IsNullOrEmpty(query) ? baseUri : baseUri + separator + query;
        context.Response.Redirect(redirectUrl);
    }

    private async Task<XeroStoredToken?> LoadTokenAsync(CancellationToken cancellationToken)
    {
        XeroAccess? setting = await _db.XeroAccesses.FirstOrDefaultAsync(x => x.Expires > DateTime.UtcNow, cancellationToken);

        if (setting == null || string.IsNullOrWhiteSpace(setting.Token))
            return null;

        try
        {
            return JsonSerializer.Deserialize<XeroStoredToken>(setting.Token);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Saves the token to the database.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task SaveRefreshTokenAsync(XeroStoredToken token, CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(token);
        XeroAccess? setting = await _db.XeroAccesses.FirstOrDefaultAsync(x => x.Expires > DateTime.UtcNow, cancellationToken);
        DateTime now = DateTime.UtcNow;
        if (setting == null)
        {
            setting = new XeroAccess
            {
                Token = json,
                DateRefreshed = now,
                Expires = now.AddDays(_options.TokenExpiryDays),
            };
            await _db.XeroAccesses.AddAsync(setting, cancellationToken);
        }
        else
        {
            setting.Token = json;
            setting.DateRefreshed = now;
        }
        await _db.SaveChangesAsync(cancellationToken);
    }
}
