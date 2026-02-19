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
        TimeSheetDto entry = new(0, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, 0, 1, "Test entry", "");
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/timesheet", entry);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        TimeSheetDto? resValue = await response.Content.ReadFromJsonAsync<TimeSheetDto>();
        Assert.NotNull(resValue);
        Assert.Equal(entry.Start.Date, resValue.Start.Date);
        Assert.Equal(entry.End?.Date, resValue.End?.Date);
        Assert.Equal(entry.Description, resValue.Description);
    }

    [Fact]
    public async Task Get_timesheet_returns_ok()
    {
        TimeSheetDto entry = new(0, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, 0, 1, "Test entry", "");
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

    [Fact]
    public async Task Update_timesheet_entry_success()
    {
        // Create a timesheet entry with no end time
        TimeSheetDto entry = new(0, DateTime.UtcNow.AddHours(-1), null, 0, 1, "Test entry", "");
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/timesheet", entry);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        TimeSheetDto? resValue = await response.Content.ReadFromJsonAsync<TimeSheetDto>();
        Assert.NotNull(resValue);
        Assert.Equal(entry.Description, resValue.Description);
        Assert.Equal(entry.Start.Date, resValue.Start.Date);
        Assert.Null(resValue.End);

        TimeSheetDto updatevalue = resValue with { Description = "Updated description", End = DateTime.UtcNow };
        response = await _client.PutAsJsonAsync($"/api/timesheet", updatevalue);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        TimeSheetDto? updateResponse = await response.Content.ReadFromJsonAsync<TimeSheetDto>();
        Assert.NotNull(updateResponse);
        Assert.Equal(updatevalue.Description, updateResponse.Description);
        Assert.Equal(updatevalue.Start, updateResponse.Start);
        Assert.Equal(updatevalue.End?.Date, updateResponse.End?.Date);
    }

    [Fact]
    public async Task Delete_timesheet_entry_success()
    {
        TimeSheetDto entry = new(0, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, 0, 1, "Test entry", "");
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/timesheet", entry);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        TimeSheetDto? resValue = await response.Content.ReadFromJsonAsync<TimeSheetDto>();
        Assert.NotNull(resValue);
        response = await _client.DeleteAsync($"/api/timesheet/{resValue.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string start = DateTime.UtcNow.AddDays(-2).ToString("O");
        HttpResponseMessage getResponse = await _client.GetAsync($"/api/timesheet/1?start={Uri.EscapeDataString(start)}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        TimeSheetDto[]? getValue = await getResponse.Content.ReadFromJsonAsync<TimeSheetDto[]>();
        Assert.NotNull(getValue);
        Assert.True(getValue.Length == 0 || !getValue.Any(e => e.Id == resValue.Id), "The deleted timesheet entry should not be present in the timesheet list");
    }

    [Fact]
    public async Task Delete_timesheet_entry_badrequest()
    {
        HttpResponseMessage response = await _client.DeleteAsync($"/api/timesheet/{1000}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
