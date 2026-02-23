using Microsoft.AspNetCore.Http;
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
    public async Task Create_job_returns_success_status()
    {
        JobCreationDto dto = new()
        {
            JobNumber = 123123,
            JobType = Shared.JobTypeEnum.Construction,
            ContactId = 1,
            Details = "Test",
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/jobs", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        int? newJobId = await response.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(newJobId);
        Assert.NotEqual(0, newJobId);

        response = await _client.GetAsync($"/api/jobs/{newJobId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JobDetailsDto? job = await response.Content.ReadFromJsonAsync<JobDetailsDto>();
        Assert.NotNull(job);
        Assert.Equal(dto.Details, job.Details);
        Assert.Equal(dto.ContactId, job.Contact?.ContactId);
        Assert.Equal(dto.JobNumber, job.JobNumber);
        Assert.Equal(dto.JobType, job.JobType);
    }
    [Fact]
    public async Task Create_job_bad_request()
    {
        JobCreationDto invalidCreatJobRequest = new()
        {
            JobNumber = 0,
            JobType = Shared.JobTypeEnum.Construction,
            ContactId = 1,
            Details = "Test",
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/jobs", invalidCreatJobRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string textResponse = await response.Content.ReadAsStringAsync();

        invalidCreatJobRequest.ContactId = 213123;
        response = await _client.PostAsJsonAsync("/api/jobs", invalidCreatJobRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        textResponse = await response.Content.ReadAsStringAsync();

        invalidCreatJobRequest.ContactId = 1;
        invalidCreatJobRequest.CouncilId = 10000;
        response = await _client.PostAsJsonAsync("/api/jobs", invalidCreatJobRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        textResponse = await response.Content.ReadAsStringAsync();
    }


    [Fact]
    public async Task Update_job_returns_success()
    {
        JobCreationDto dto = new()
        {
            JobNumber = 1231231,
            JobType = Shared.JobTypeEnum.Construction,
            ContactId = 1,
            Details = "Test",
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/jobs", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        int? newJobId = await response.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(newJobId);
        Assert.NotEqual(0, newJobId);

        response = await _client.GetAsync($"/api/jobs/{newJobId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JobDetailsDto? job = await response.Content.ReadFromJsonAsync<JobDetailsDto>();
        Assert.NotNull(job);
        Assert.Equal(dto.Details, job.Details);
        Assert.Equal(dto.ContactId, job.Contact?.ContactId);
        Assert.Equal(dto.JobNumber, job.JobNumber);
        Assert.Equal(dto.JobType, job.JobType);

        job.Details = "Updated Job Details";
        job.JobType = Shared.JobTypeEnum.Surveying;
        response = await _client.PutAsJsonAsync("/api/jobs", job);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JobDetailsDto? updateJob = await response.Content.ReadFromJsonAsync<JobDetailsDto?>();
        Assert.NotNull(updateJob);
        Assert.Equal(job.Details, updateJob.Details);
        Assert.Equal(job.JobType, updateJob.JobType);
    }

    [Fact]
    public async Task Delete_job_returns_success()
    {
        JobCreationDto dto = new()
        {
            JobNumber = 11111,
            JobType = Shared.JobTypeEnum.Construction,
            ContactId = 1,
            Details = "Test",
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/jobs", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        int? newJobId = await response.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(newJobId);
        Assert.NotEqual(0, newJobId);

        response = await _client.GetAsync($"/api/jobs/{newJobId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JobDetailsDto? job = await response.Content.ReadFromJsonAsync<JobDetailsDto>();
        Assert.NotNull(job);
        Assert.Equal(dto.Details, job.Details);
        Assert.Equal(dto.ContactId, job.Contact?.ContactId);
        Assert.Equal(dto.JobNumber, job.JobNumber);
        Assert.Equal(dto.JobType, job.JobType);

        job.Details = "Updated Job Details";
        job.JobType = Shared.JobTypeEnum.Surveying;
        response = await _client.DeleteAsync($"/api/jobs/{job.JobId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        response = await _client.GetAsync("/api/jobs?page=1&pageSize=10");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PagedResponse<ListJobDto>? resValue = await response.Content.ReadFromJsonAsync<PagedResponse<ListJobDto>>();
        Assert.NotNull(resValue);
        Assert.DoesNotContain(resValue.Result, x => x.JobNumber == job.JobNumber);
    }

    [Fact]
    public async Task Get_assigned_user_notes_returns_ok_or_bad_request()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/jobs/assignedUserNotes/1");
        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.BadRequest or HttpStatusCode.NotFound,
            $"Unexpected status: {response.StatusCode}");
    }
}
