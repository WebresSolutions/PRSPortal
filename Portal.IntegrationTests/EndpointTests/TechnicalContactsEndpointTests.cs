using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Job;
using System.Net;
using System.Net.Http.Json;

namespace Portal.IntegrationTests.EndpointTests;

[Collection(nameof(IntegrationTestCollection))]
public sealed class TechnicalContactsEndpointTests
{
    private readonly HttpClient _client;

    public TechnicalContactsEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Get_technical_contacts_without_job_or_contact_returns_bad_request()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/jobs/technical-contacts");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_technical_contacts_by_job_id_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/jobs/technical-contacts?jobId=1");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        TechnicalContactDto[]? contacts = await response.Content.ReadFromJsonAsync<TechnicalContactDto[]>();
        Assert.NotNull(contacts);
    }

    [Fact]
    public async Task Get_technical_contacts_by_contact_id_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/jobs/technical-contacts?contactId=1");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        TechnicalContactDto[]? contacts = await response.Content.ReadFromJsonAsync<TechnicalContactDto[]>();
        Assert.NotNull(contacts);
    }

    [Fact]
    public async Task Create_technical_contact_returns_success()
    {
        JobCreationDto jobDto = new()
        {
            JobType = [Shared.JobTypeEnum.Construction],
            ContactId = 1,
            Details = "Job for technical contact tests",
        };

        HttpResponseMessage createJobResponse = await _client.PostAsJsonAsync("/api/jobs", jobDto);
        createJobResponse.EnsureSuccessStatusCode();
        int? jobId = await createJobResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(jobId);
        Assert.True(jobId > 0);

        SaveTechnicalContactTypeDto createDto = new()
        {
            ContactId = 1,
            JobId = jobId.Value,
            ContactTypeId = 1,
        };

        HttpResponseMessage createTcResponse = await _client.PutAsJsonAsync("/api/jobs/technical-contacts", createDto);
        Assert.Equal(HttpStatusCode.OK, createTcResponse.StatusCode);

        HttpResponseMessage getResponse = await _client.GetAsync($"/api/jobs/technical-contacts?jobId={jobId}");
        getResponse.EnsureSuccessStatusCode();
        TechnicalContactDto[]? getList = await getResponse.Content.ReadFromJsonAsync<TechnicalContactDto[]>();
        Assert.NotNull(getList);
        Assert.Contains(getList, tc => tc.ContactId == createDto.ContactId);
    }

    [Fact]
    public async Task Update_technical_contact_returns_success()
    {
        JobCreationDto jobDto = new()
        {
            JobType = [Shared.JobTypeEnum.Construction],
            ContactId = 1,
            Details = "Job for update technical contact tests",
        };

        HttpResponseMessage createJobResponse = await _client.PostAsJsonAsync("/api/jobs", jobDto);
        createJobResponse.EnsureSuccessStatusCode();
        int? jobId = await createJobResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(jobId);
        Assert.True(jobId > 0);

        SaveTechnicalContactTypeDto createDto = new()
        {
            ContactId = 1,
            JobId = jobId.Value,
            ContactTypeId = 1,
        };

        HttpResponseMessage createTcResponse = await _client.PutAsJsonAsync("/api/jobs/technical-contacts", createDto);
        createTcResponse.EnsureSuccessStatusCode();

        TechnicalContactDto[]? getReponse = await _client.GetFromJsonAsync<TechnicalContactDto[]>($"/api/jobs/technical-contacts?jobId={jobId}");
        Assert.NotNull(getReponse);
        TechnicalContactDto? created = getReponse.FirstOrDefault(x => x.ContactId == 1 && x.JobId == jobId);
        Assert.NotNull(created);

        SaveTechnicalContactTypeDto updateDto = new()
        {
            Id = created.Id,
            ContactId = created.ContactId,
            JobId = jobId.Value,
            ContactTypeId = 2,
        };

        HttpResponseMessage updateTcResponse = await _client.PutAsJsonAsync("/api/jobs/technical-contacts", updateDto);
        Assert.Equal(HttpStatusCode.OK, updateTcResponse.StatusCode);
        getReponse = await _client.GetFromJsonAsync<TechnicalContactDto[]>($"/api/jobs/technical-contacts?jobId={jobId}");
        Assert.NotNull(getReponse);
        TechnicalContactDto? updated = getReponse.FirstOrDefault(tc => tc.Id == created.Id);
        Assert.NotNull(updated);
        Assert.Equal(2, updated.ContactTypeId);
        Assert.Equal(jobId.Value, updated.JobId);
    }

    [Fact]
    public async Task Create_technical_contact_with_invalid_job_returns_bad_request()
    {
        SaveTechnicalContactTypeDto dto = new()
        {
            ContactId = 1,
            JobId = 999999,
            ContactTypeId = 1,
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/jobs/technical-contacts", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_technical_contact_with_invalid_contact_returns_bad_request()
    {
        JobCreationDto jobDto = new()
        {
            JobType = [Shared.JobTypeEnum.Construction],
            ContactId = 1,
            Details = "Job for invalid contact test",
        };

        HttpResponseMessage createJobResponse = await _client.PostAsJsonAsync("/api/jobs", jobDto);
        createJobResponse.EnsureSuccessStatusCode();
        int? jobId = await createJobResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(jobId);

        SaveTechnicalContactTypeDto dto = new()
        {
            ContactId = 999999,
            JobId = jobId.Value,
            ContactTypeId = 1,
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/jobs/technical-contacts", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
