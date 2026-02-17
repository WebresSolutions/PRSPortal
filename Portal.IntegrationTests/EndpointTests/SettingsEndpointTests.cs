using System.Net;

namespace Portal.IntegrationTests.EndpointTests;

[Collection(nameof(IntegrationTestCollection))]
public sealed class SettingsEndpointTests
{
    private readonly HttpClient _client;

    public SettingsEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Get_system_settings_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/settings/systemsettings");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

}
