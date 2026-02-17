using Microsoft.Extensions.DependencyInjection;
using Portal.Data;
using Portal.Shared.DTO.Schedule;
using System.Net;
using System.Net.Http.Json;

namespace Portal.IntegrationTests.EndpointTests;

[Collection(nameof(IntegrationTestCollection))]
public sealed class ScheduleEndpointTests
{
    private readonly HttpClient _client;
    private readonly PrsDbContext _PrsDbContext;

    public ScheduleEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
        _PrsDbContext = fixture.Factory.Services.GetService<PrsDbContext>() ?? throw new Exception("Failed to find database context");
    }

    [Fact]
    public async Task Get_slots_returns_ok_or_bad_request()
    {
        // Create a new slot 

        HttpResponseMessage response = await _client.GetAsync("/api/schedule/slots?date=2025-02-17&jobType=1");
        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.BadRequest,
            $"Unexpected status: {response.StatusCode}");
        List<ScheduleSlotDTO>? data = await response.Content.ReadFromJsonAsync<List<ScheduleSlotDTO>>();
        Assert.NotNull(data);
        Assert.True(data.Count() >= 4);
    }

    [Fact]
    public async Task Get_colours_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/schedule/colours");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Put_colours_with_invalid_body_returns_bad_request()
    {
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/schedule/colours", new ScheduleColourDto { ColourHex = "invalid" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
