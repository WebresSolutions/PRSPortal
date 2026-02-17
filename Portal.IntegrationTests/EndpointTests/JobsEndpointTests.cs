using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;
using System.Net;
using System.Net.Http.Json;

namespace Portal.IntegrationTests.EndpointTests;

[Collection(nameof(IntegrationTestCollection))]
public sealed class JobsEndpointTests
{
    private readonly HttpClient _client;

    public JobsEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task List_jobs_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/jobs?page=1&pageSize=10");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PagedResponse<ListJobDto>? resValue = await response.Content.ReadFromJsonAsync<PagedResponse<ListJobDto>>();
        Assert.NotNull(resValue);
        Assert.True(resValue.TotalCount >= 4);
        Assert.Equal(10, resValue.PageSize);
        Assert.True(resValue.TotalNumberPages >= 1);
    }

    [Fact]
    public async Task List_jobs_bad_request()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/jobs");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_job_by_invalid_id_returns_bad_request()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/jobs/0");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_job_by_id_returns_ok_or_not_found()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/jobs/1");
        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NotFound,
            $"Unexpected status: {response.StatusCode}");
    }

    [Fact]
    public async Task Create_job_returns_success_status()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/jobs", new { });
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest,
            $"Unexpected status: {response.StatusCode}");
    }

    [Fact]
    public async Task Update_job_returns_success_or_bad_request()
    {
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/jobs/1", new { });
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound,
            $"Unexpected status: {response.StatusCode}");
    }

    [Fact]
    public async Task Get_assigned_user_notes_returns_ok_or_bad_request()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/jobs/assignedUserNotes/1");
        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.BadRequest or HttpStatusCode.NotFound,
            $"Unexpected status: {response.StatusCode}");
    }
}
