using Portal.Shared.DTO.TimeSheet;
using System.Net;
using System.Net.Http.Json;

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
    public async Task Post_timesheet_entry_returns_ok()
    {
        TimeSheetEntryDto entry = new(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, "Test entry", 1, null);
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/timesheet", entry);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        entry = new(DateTime.UtcNow.AddHours(-4), null, "Test entry on a timer", 1, null);
        response = await _client.PostAsJsonAsync("/api/timesheet", entry);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Cannot have a second entry with a null end time for the same user
        entry = new(DateTime.UtcNow.AddHours(-4), null, "Test entry", 1, null);
        response = await _client.PostAsJsonAsync("/api/timesheet", entry);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_timesheet_returns_ok()
    {
        TimeSheetEntryDto entry = new(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, "Test entry", 1, null);
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/timesheet", entry);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string start = DateTime.UtcNow.AddDays(-2).ToString("O");
        response = await _client.GetAsync($"/api/timesheet/1?start={Uri.EscapeDataString(start)}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        TimeSheetDto[]? resValue = await response.Content.ReadFromJsonAsync<TimeSheetDto[]>();
        Assert.NotNull(resValue);
        Assert.True(resValue.Length >= 1, "There should be at least 1 entry in the timesheets");
    }

    [Fact]
    public async Task Get_timesheet_returns_badreq()
    {
        // End date is before the start date
        string start = DateTime.UtcNow.AddDays(-2).ToString("O");
        string end = DateTime.UtcNow.AddDays(-4).ToString("O");

        HttpResponseMessage response = await _client.GetAsync($"/api/timesheet/1?start={Uri.EscapeDataString(start)}&end={Uri.EscapeDataString(end)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
