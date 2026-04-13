using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Portal.Client.Services.Interfaces;

namespace Portal.Client.Services.Instances;

public sealed class EntraProfilePhotoService : IEntraProfilePhotoService
{
    private static readonly AccessTokenRequestOptions GraphTokenOptions = new()
    {
        Scopes = ["https://graph.microsoft.com/User.Read"],
    };

    private readonly HttpClient _httpClient;
    private readonly IAccessTokenProvider _tokenProvider;

    public EntraProfilePhotoService(HttpClient httpClient, IAccessTokenProvider tokenProvider)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
    }

    /// <inheritdoc />
    public async Task<string?> GetProfilePhotoDataUriAsync(CancellationToken cancellationToken = default)
    {
        AccessTokenResult tokenResult = await _tokenProvider.RequestAccessToken(GraphTokenOptions);

        if (!tokenResult.TryGetToken(out AccessToken? accessToken))
            return null;

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://graph.microsoft.com/v1.0/me/photo/$value");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            return null;

        byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        if (bytes.Length == 0)
            return null;

        string mediaType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
        return $"data:{mediaType};base64,{Convert.ToBase64String(bytes)}";
    }
}
