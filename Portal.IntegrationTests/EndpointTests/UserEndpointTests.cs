using Portal.Shared.DTO.User;
using System.Net;
using System.Net.Http.Json;

namespace Portal.IntegrationTests.EndpointTests;

[Collection(nameof(IntegrationTestCollection))]
public sealed class UserEndpointTests
{
    private readonly HttpClient _client;

    public UserEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Get_users_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/users");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        UserDto[]? resValue = await response.Content.ReadFromJsonAsync<UserDto[]>();
        Assert.NotNull(resValue);
        Assert.True(resValue.Length > 0);
    }

    [Fact]
    public async Task Get_users_active_only_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/users?activeOnly=true");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
