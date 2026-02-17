using System.Net;

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
    }

    [Fact]
    public async Task Get_users_active_only_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/users?activeOnly=true");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
