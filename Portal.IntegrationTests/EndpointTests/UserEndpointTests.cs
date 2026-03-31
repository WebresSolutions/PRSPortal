using Portal.Shared;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.User;
using System.Net;
using System.Net.Http.Json;

namespace Portal.IntegrationTests.EndpointTests;

[Collection(nameof(IntegrationTestCollection))]
public sealed class UserEndpointTests
{
    private readonly HttpClient _client;

    public UserEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    private static JobCreationDto CreateJobDto(string details, int responsibleTeamMember) =>
        new()
        {
            JobType = [JobTypeEnum.Construction],
            ContactId = 1,
            Details = details,
            StatusId = 1,
            ResponsibleTeamMember = responsibleTeamMember,
            Address = new()
            {
                Street = "456 User Jobs Test St",
                State = StateEnum.NSW,
                LatLng = new() { Latitude = -37.8, Longitude = 144.9 },
                PostCode = "3000",
                Suburb = "Melbourne",
            }
        };

    private static JobUpdateDto JobDetailsToUpdateDto(JobDetailsDto job, int? responsibleTeamMember) =>
        new()
        {
            JobId = job.JobId,
            Address = job.Address,
            ContactId = job.ContactId,
            JobTypes = job.JobTypes,
            JobColourId = job.JobColourId,
            JobStatusId = job.JobStatusId,
            Details = job.Description,
            CouncilId = job.CouncilId,
            ResponsibleTeamMember = responsibleTeamMember,
        };

    [Fact]
    public async Task Get_users_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/users");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        UserDto[]? resValue = await response.Content.ReadFromJsonAsync<UserDto[]>();
        Assert.NotNull(resValue);
        Assert.True(resValue.Length > 0);
    }

    [Fact]
    public async Task Get_users_active_only_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/users?activeOnly=true");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_user_jobs_returns_ok_and_includes_job_when_responsible_team_member_is_assigned()
    {
        const int assignedUserId = 2;
        JobCreationDto createDto = CreateJobDto("User jobs list — assigned via responsible team member", assignedUserId);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/jobs", createDto);
        createResponse.EnsureSuccessStatusCode();
        int? jobId = await createResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(jobId);
        Assert.True(jobId > 0);

        HttpResponseMessage jobsResponse = await _client.GetAsync($"/api/users/jobs/{assignedUserId}");
        jobsResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, jobsResponse.StatusCode);

        UserJobsListDto? list = await jobsResponse.Content.ReadFromJsonAsync<UserJobsListDto>();
        Assert.NotNull(list);
        Assert.Equal(assignedUserId, list.UserId);
        Assert.Contains(list.UserJobs, j => j.JobId == jobId.Value);
        UserJobDto? entry = list.UserJobs.FirstOrDefault(j => j.JobId == jobId.Value);
        Assert.NotNull(entry?.JobDetails?.JobNumber);
    }

    [Fact]
    public async Task Get_user_jobs_returns_empty_list_when_user_has_no_job_assignments()
    {
        const int unassignedUserId = 999_999;
        HttpResponseMessage response = await _client.GetAsync($"/api/users/jobs/{unassignedUserId}");
        response.EnsureSuccessStatusCode();
        UserJobsListDto? list = await response.Content.ReadFromJsonAsync<UserJobsListDto>();
        Assert.NotNull(list);
        Assert.Equal(unassignedUserId, list.UserId);
        Assert.Empty(list.UserJobs);
    }

    [Fact]
    public async Task Get_user_jobs_userId_zero_resolves_to_test_user_from_context()
    {
        // PortalWebApplicationFactory sets Testing:TestUserId = 1
        JobCreationDto createDto = CreateJobDto("User jobs — user id 0 resolves to test user", responsibleTeamMember: 1);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/jobs", createDto);
        createResponse.EnsureSuccessStatusCode();
        int? jobId = await createResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(jobId);

        HttpResponseMessage response = await _client.GetAsync("/api/users/jobs/0");
        response.EnsureSuccessStatusCode();
        UserJobsListDto? list = await response.Content.ReadFromJsonAsync<UserJobsListDto>();
        Assert.NotNull(list);
        Assert.Equal(1, list.UserId);
        Assert.Contains(list.UserJobs, j => j.JobId == jobId.Value);
    }

    [Fact]
    public async Task Update_job_responsible_team_member_is_reflected_in_get_user_jobs()
    {
        JobCreationDto createDto = CreateJobDto("User jobs — reassign via update job", responsibleTeamMember: 2);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/jobs", createDto);
        createResponse.EnsureSuccessStatusCode();
        int? jobId = await createResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(jobId);

        HttpResponseMessage getJobResponse = await _client.GetAsync($"/api/jobs/{jobId}");
        getJobResponse.EnsureSuccessStatusCode();
        JobDetailsDto? job = await getJobResponse.Content.ReadFromJsonAsync<JobDetailsDto>();
        Assert.NotNull(job);

        HttpResponseMessage user2Before = await _client.GetAsync("/api/users/jobs/2");
        user2Before.EnsureSuccessStatusCode();
        UserJobsListDto? list2Before = await user2Before.Content.ReadFromJsonAsync<UserJobsListDto>();
        Assert.NotNull(list2Before);
        Assert.Contains(list2Before.UserJobs, j => j.JobId == jobId.Value);

        JobUpdateDto updateDto = JobDetailsToUpdateDto(job, responsibleTeamMember: 1);
        HttpResponseMessage updateResponse = await _client.PutAsJsonAsync("/api/jobs", updateDto);
        updateResponse.EnsureSuccessStatusCode();

        HttpResponseMessage user1After = await _client.GetAsync("/api/users/jobs/1");
        user1After.EnsureSuccessStatusCode();
        UserJobsListDto? list1After = await user1After.Content.ReadFromJsonAsync<UserJobsListDto>();
        Assert.NotNull(list1After);
        Assert.Contains(list1After.UserJobs, j => j.JobId == jobId.Value);

        HttpResponseMessage user2After = await _client.GetAsync("/api/users/jobs/2");
        user2After.EnsureSuccessStatusCode();
        UserJobsListDto? list2After = await user2After.Content.ReadFromJsonAsync<UserJobsListDto>();
        Assert.NotNull(list2After);
        Assert.DoesNotContain(list2After.UserJobs, j => j.JobId == jobId.Value);
    }
}
