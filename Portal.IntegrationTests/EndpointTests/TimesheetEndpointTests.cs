using System.Net;

namespace Portal.IntegrationTests.EndpointTests;

[Collection(nameof(IntegrationTestCollection))]
public sealed class TimesheetEndpointTests
{
    private readonly HttpClient _client;

    public TimesheetEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Get_timesheet_returns_ok_or_bad_request()
    {
        string start = DateTime.UtcNow.AddDays(-7).ToString("O");
        HttpResponseMessage response = await _client.GetAsync($"/api/timesheet/1?start={Uri.EscapeDataString(start)}");
        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.BadRequest or HttpStatusCode.NotFound,
            $"Unexpected status: {response.StatusCode}");
    }
}
