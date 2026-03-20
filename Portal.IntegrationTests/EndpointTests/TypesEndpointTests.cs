using System.Net;
using System.Net.Http.Json;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.DTO.Setting;
using Portal.Shared.DTO.TimeSheet;

namespace Portal.IntegrationTests.EndpointTests;

[Collection(nameof(IntegrationTestCollection))]
public sealed class TypesEndpointTests
{
    private readonly HttpClient _client;

    public TypesEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Get_all_settings_types_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/types/all");
        response.EnsureSuccessStatusCode();
        AllSettingsTypesDto? bundle = await response.Content.ReadFromJsonAsync<AllSettingsTypesDto>();
        Assert.NotNull(bundle);
        Assert.NotNull(bundle.TimesheetTypes);
        Assert.NotNull(bundle.ScheduleColours);
        Assert.NotNull(bundle.ServiceTypes);
    }

    [Fact]
    public async Task Get_timesheet_types_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/types/timesheet");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_timesheet_types_returns_array()
    {
        TimeTypeDto[]? result = await _client.GetFromJsonAsync<TimeTypeDto[]>("/api/types/timesheet");
        Assert.NotNull(result);
        Assert.True(result.Length >= 2, "Seed data should include at least 2 timesheet types.");
    }

    [Fact]
    public async Task Get_timesheet_types_returns_valid_dto_shape()
    {
        TimeTypeDto[]? result = await _client.GetFromJsonAsync<TimeTypeDto[]>("/api/types/timesheet");
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        foreach (TimeTypeDto dto in result)
        {
            Assert.True(dto.Id > 0);
            Assert.False(string.IsNullOrWhiteSpace(dto.Name));
        }
    }

    [Fact]
    public async Task Get_contact_types_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/types/contact");
        response.EnsureSuccessStatusCode();
        ContactTypeDto[]? result = await response.Content.ReadFromJsonAsync<ContactTypeDto[]>();
        Assert.NotNull(result);
        Assert.True(result.Length >= 2);
    }

    [Fact]
    public async Task Get_job_types_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/types/job");
        response.EnsureSuccessStatusCode();
        JobTypeDto[]? result = await response.Content.ReadFromJsonAsync<JobTypeDto[]>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Get_job_colours_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/types/jobcolour");
        response.EnsureSuccessStatusCode();
        JobColourDto[]? result = await response.Content.ReadFromJsonAsync<JobColourDto[]>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Get_schedule_colours_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/types/schedulecolour");
        response.EnsureSuccessStatusCode();
        ScheduleColourDto[]? result = await response.Content.ReadFromJsonAsync<ScheduleColourDto[]>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Get_file_types_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/types/file");
        response.EnsureSuccessStatusCode();
        FileTypeDto[]? result = await response.Content.ReadFromJsonAsync<FileTypeDto[]>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Get_job_task_types_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/types/jobtask");
        response.EnsureSuccessStatusCode();
        JobTaskTypeDto[]? result = await response.Content.ReadFromJsonAsync<JobTaskTypeDto[]>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Get_technical_contact_types_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/types/technicalcontact");
        response.EnsureSuccessStatusCode();
        TechnicalContactTypeDto[]? result = await response.Content.ReadFromJsonAsync<TechnicalContactTypeDto[]>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Get_states_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/types/state");
        response.EnsureSuccessStatusCode();
        StateDto[]? result = await response.Content.ReadFromJsonAsync<StateDto[]>();
        Assert.NotNull(result);
        Assert.True(result.Length >= 7, "Seed data includes 7 Australian states/territories.");
    }
}
