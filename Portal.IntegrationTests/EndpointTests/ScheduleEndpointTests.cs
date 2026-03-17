using Microsoft.Extensions.DependencyInjection;
using Portal.Data;
using Portal.Shared;
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
        List<ScheduleTrackDto>? data = await response.Content.ReadFromJsonAsync<List<ScheduleTrackDto>>();
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

    [Fact]
    public async Task Get_week_returns_ok_and_array()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/schedule/week?jobType=1");
        Assert.True(response.IsSuccessStatusCode, $"Unexpected status: {response.StatusCode}");
        WeeklyScheduleDto[]? data = await response.Content.ReadFromJsonAsync<WeeklyScheduleDto[]>();
        Assert.NotNull(data);
    }

    [Fact]
    public async Task Get_week_with_weekDay_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/schedule/week?jobType=1&weekDay=2026-03-13");
        Assert.True(response.IsSuccessStatusCode, $"Unexpected status: {response.StatusCode}");
        WeeklyScheduleDto[]? data = await response.Content.ReadFromJsonAsync<WeeklyScheduleDto[]>();
        Assert.NotNull(data);
    }

    [Fact]
    public async Task Put_schedule_with_invalid_track_returns_bad_request()
    {
        UpdateScheduleDto dto = new()
        {
            Id = 0,
            TrackId = 99999,
            Start = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(8)),
            End = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(17)),
            ColourId = 1,
            Notes = "Test"
        };
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/schedule", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_schedule_with_start_after_end_returns_bad_request()
    {
        HttpResponseMessage slotsResp = await _client.GetAsync($"/api/schedule/slots?date={DateTime.UtcNow:yyyy-MM-dd}&jobType=1");
        slotsResp.EnsureSuccessStatusCode();
        List<ScheduleTrackDto>? slots = await slotsResp.Content.ReadFromJsonAsync<List<ScheduleTrackDto>>();
        Assert.NotNull(slots);
        Assert.True(slots.Count > 0);
        HttpResponseMessage coloursResp = await _client.GetAsync("/api/schedule/colours");
        coloursResp.EnsureSuccessStatusCode();
        List<ScheduleColourDto>? colours = await coloursResp.Content.ReadFromJsonAsync<List<ScheduleColourDto>>();
        Assert.NotNull(colours);
        Assert.True(colours.Count > 0);

        UpdateScheduleDto dto = new()
        {
            Id = 0,
            TrackId = slots[0].TrackId,
            Start = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(14)),
            End = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(8)),
            ColourId = colours[0].ScheduleColourId,
            Notes = "Test"
        };
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/schedule", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_schedule_with_span_over_12_hours_returns_bad_request()
    {
        HttpResponseMessage slotsResp = await _client.GetAsync($"/api/schedule/slots?date={DateTime.UtcNow:yyyy-MM-dd}&jobType=1");
        slotsResp.EnsureSuccessStatusCode();
        List<ScheduleTrackDto>? slots = await slotsResp.Content.ReadFromJsonAsync<List<ScheduleTrackDto>>();
        Assert.NotNull(slots);
        Assert.True(slots.Count > 0);
        HttpResponseMessage coloursResp = await _client.GetAsync("/api/schedule/colours");
        coloursResp.EnsureSuccessStatusCode();
        List<ScheduleColourDto>? colours = await coloursResp.Content.ReadFromJsonAsync<List<ScheduleColourDto>>();
        Assert.NotNull(colours);
        Assert.True(colours.Count > 0);

        UpdateScheduleDto dto = new()
        {
            Id = 0,
            TrackId = slots[0].TrackId,
            Start = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(8)),
            End = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(21)),
            ColourId = colours[0].ScheduleColourId,
            Notes = "Test"
        };
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/schedule", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_schedule_create_returns_ok_and_id()
    {
        // Ensure we have at least one track (slots endpoint creates 4 if none exist)
        HttpResponseMessage slotsResp = await _client.GetAsync($"/api/schedule/slots?date={DateTime.UtcNow:yyyy-MM-dd}&jobType=1");
        slotsResp.EnsureSuccessStatusCode();
        List<ScheduleTrackDto>? slots = await slotsResp.Content.ReadFromJsonAsync<List<ScheduleTrackDto>>();
        Assert.NotNull(slots);
        Assert.True(slots.Count > 0);

        HttpResponseMessage coloursResp = await _client.GetAsync("/api/schedule/colours");
        coloursResp.EnsureSuccessStatusCode();
        List<ScheduleColourDto>? colours = await coloursResp.Content.ReadFromJsonAsync<List<ScheduleColourDto>>();
        Assert.NotNull(colours);
        Assert.True(colours.Count > 0);

        UpdateScheduleDto dto = new()
        {
            Id = 0,
            TrackId = slots[0].TrackId,
            Start = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(8)),
            End = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(12)),
            ColourId = colours[0].ScheduleColourId,
            Notes = "Integration test schedule"
        };
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/schedule", dto);
        Assert.True(response.IsSuccessStatusCode, $"Unexpected status: {response.StatusCode}");
        int? id = await response.Content.ReadFromJsonAsync<int>();
        Assert.NotNull(id);
        Assert.True(id > 0);

        slotsResp = await _client.GetAsync($"/api/schedule/slots?date={DateTime.UtcNow:yyyy-MM-dd}&jobType=1");
        slotsResp.EnsureSuccessStatusCode();
        slots = await slotsResp.Content.ReadFromJsonAsync<List<ScheduleTrackDto>>();
        Assert.NotNull(slots);
        ScheduleDto? newlyCreatedSchedule = slots.SelectMany(x => x.Schedule).FirstOrDefault(x => x.ScheduleId == id && x.ScheduleTrackId == dto.TrackId);
        Assert.NotNull(newlyCreatedSchedule);
    }

    [Fact]
    public async Task Put_schedule_update_returns_ok_and_persists_changes()
    {
        // Create a schedule first
        HttpResponseMessage slotsResp = await _client.GetAsync($"/api/schedule/slots?date={DateTime.UtcNow:yyyy-MM-dd}&jobType=1");
        slotsResp.EnsureSuccessStatusCode();
        List<ScheduleTrackDto>? slots = await slotsResp.Content.ReadFromJsonAsync<List<ScheduleTrackDto>>();
        Assert.NotNull(slots);
        Assert.True(slots.Count > 0);

        HttpResponseMessage coloursResp = await _client.GetAsync("/api/schedule/colours");
        coloursResp.EnsureSuccessStatusCode();
        List<ScheduleColourDto>? colours = await coloursResp.Content.ReadFromJsonAsync<List<ScheduleColourDto>>();
        Assert.NotNull(colours);
        Assert.True(colours.Count > 0);

        int trackId = slots[0].TrackId;
        int colourId = colours.Count > 1 ? colours[1].ScheduleColourId : colours[0].ScheduleColourId;

        UpdateScheduleDto createDto = new()
        {
            Id = 0,
            TrackId = trackId,
            Start = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(8)),
            End = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(12)),
            ColourId = colours[0].ScheduleColourId,
            Notes = "Original description"
        };
        HttpResponseMessage createResponse = await _client.PutAsJsonAsync("/api/schedule", createDto);
        createResponse.EnsureSuccessStatusCode();
        int scheduleId = await createResponse.Content.ReadFromJsonAsync<int>();
        Assert.True(scheduleId > 0);

        // Update the schedule
        const string updatedDescription = "Updated by integration test";
        TimeOnly updatedStart = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(9));
        TimeOnly updatedEnd = TimeOnly.FromDateTime(DateTime.UtcNow.Date.AddHours(14));
        UpdateScheduleDto updateDto = new()
        {
            Id = scheduleId,
            TrackId = trackId,
            Start = updatedStart,
            End = updatedEnd,
            ColourId = colourId,
            Notes = updatedDescription
        };
        HttpResponseMessage updateResponse = await _client.PutAsJsonAsync("/api/schedule", updateDto);
        Assert.True(updateResponse.IsSuccessStatusCode, $"Unexpected status: {updateResponse.StatusCode}");
        int? returnedId = await updateResponse.Content.ReadFromJsonAsync<int>();
        Assert.NotNull(returnedId);
        Assert.Equal(scheduleId, returnedId);

        // Verify updated values in slots
        slotsResp = await _client.GetAsync($"/api/schedule/slots?date={DateTime.UtcNow:yyyy-MM-dd}&jobType=1");
        slotsResp.EnsureSuccessStatusCode();
        slots = await slotsResp.Content.ReadFromJsonAsync<List<ScheduleTrackDto>>();
        Assert.NotNull(slots);
        ScheduleDto? updatedSchedule = slots.SelectMany(x => x.Schedule).FirstOrDefault(x => x.ScheduleId == scheduleId && x.ScheduleTrackId == trackId);
        Assert.NotNull(updatedSchedule);
        Assert.Equal(updatedDescription, updatedSchedule.Description);
        Assert.Equal(updatedStart, updatedSchedule.Start);
        Assert.Equal(updatedEnd, updatedSchedule.End);
        Assert.Equal(colourId, updatedSchedule.Colour.ScheduleColourId);
    }

    [Fact]
    public async Task Put_schedule_track_create_returns_ok_and_dto()
    {
        UpdateScheduleTrackDto dto = new()
        {
            ScheduleTrackId = 0,
            JobTypeEnum = JobTypeEnum.Construction,
            Date = new DateOnly(2025, 3, 10),
            AssignedUsers = [1]
        };
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/schedule/tracks", dto);
        Assert.True(response.IsSuccessStatusCode, $"Unexpected status: {response.StatusCode}");
        ScheduleTrackDto? track = await response.Content.ReadFromJsonAsync<ScheduleTrackDto>();
        Assert.NotNull(track);
        Assert.True(track.TrackId > 0);
        Assert.Equal(dto.Date, track.Day);
    }

    [Fact]
    public async Task Put_schedule_track_with_invalid_id_returns_bad_request()
    {
        UpdateScheduleTrackDto dto = new()
        {
            ScheduleTrackId = 99999,
            JobTypeEnum = JobTypeEnum.Construction,
            Date = new DateOnly(2025, 3, 10),
            AssignedUsers = [1]
        };
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/schedule/tracks", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_track_with_invalid_id_returns_bad_request()
    {
        HttpResponseMessage response = await _client.DeleteAsync("/api/schedule/tracks/0");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        response = await _client.DeleteAsync("/api/schedule/tracks/-1");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_track_with_nonexistent_id_returns_bad_request()
    {
        HttpResponseMessage response = await _client.DeleteAsync("/api/schedule/tracks/99999");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_track_returns_ok_and_soft_deletes()
    {
        // Create a track
        DateOnly trackDate = new(2026, 4, 15);
        UpdateScheduleTrackDto createDto = new()
        {
            ScheduleTrackId = 0,
            JobTypeEnum = JobTypeEnum.Construction,
            Date = trackDate,
            AssignedUsers = [1]
        };
        HttpResponseMessage createResponse = await _client.PutAsJsonAsync("/api/schedule/tracks", createDto);
        createResponse.EnsureSuccessStatusCode();
        ScheduleTrackDto? createdTrack = await createResponse.Content.ReadFromJsonAsync<ScheduleTrackDto>();
        Assert.NotNull(createdTrack);
        int trackId = createdTrack.TrackId;
        Assert.True(trackId > 0);

        // Confirm track appears in slots for that date
        HttpResponseMessage slotsResp = await _client.GetAsync($"/api/schedule/slots?date={trackDate:yyyy-MM-dd}&jobType=1");
        slotsResp.EnsureSuccessStatusCode();
        List<ScheduleTrackDto>? slotsBefore = await slotsResp.Content.ReadFromJsonAsync<List<ScheduleTrackDto>>();
        Assert.NotNull(slotsBefore);
        Assert.Contains(slotsBefore, t => t.TrackId == trackId);

        // Delete the track
        HttpResponseMessage deleteResponse = await _client.DeleteAsync($"/api/schedule/tracks/{trackId}");
        Assert.True(deleteResponse.IsSuccessStatusCode, $"Unexpected status: {deleteResponse.StatusCode}");
        int? returnedId = await deleteResponse.Content.ReadFromJsonAsync<int>();
        Assert.NotNull(returnedId);
        Assert.Equal(trackId, returnedId);

        // Confirm track no longer appears in slots (soft-deleted)
        slotsResp = await _client.GetAsync($"/api/schedule/slots?date={trackDate:yyyy-MM-dd}&jobType=1");
        slotsResp.EnsureSuccessStatusCode();
        List<ScheduleTrackDto>? slotsAfter = await slotsResp.Content.ReadFromJsonAsync<List<ScheduleTrackDto>>();
        Assert.NotNull(slotsAfter);
        Assert.DoesNotContain(slotsAfter, t => t.TrackId == trackId);
    }
}
