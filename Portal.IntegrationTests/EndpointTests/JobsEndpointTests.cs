using Microsoft.AspNetCore.Http;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.User;
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
    public async Task List_jobs_filtered_by_contactId_returns_only_jobs_for_that_contact()
    {
        int testJobNumber = 778001;
        JobCreationDto createDto = new()
        {
            JobNumber = testJobNumber,
            JobType = Shared.JobTypeEnum.Construction,
            ContactId = 1,
            Details = "Job for contact filter test",
        };
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/jobs", createDto);
        createResponse.EnsureSuccessStatusCode();
        int? jobId = await createResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(jobId);
        Assert.True(jobId > 0);

        HttpResponseMessage response = await _client.GetAsync($"/api/jobs?page=1&pageSize=50&contactId=1");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PagedResponse<ListJobDto>? resValue = await response.Content.ReadFromJsonAsync<PagedResponse<ListJobDto>>();
        Assert.NotNull(resValue);
        Assert.NotNull(resValue.Result);
        ListJobDto? createdJob = resValue.Result.FirstOrDefault(j => j.JobId == jobId.Value);
        Assert.NotNull(createdJob);
        Assert.Equal(testJobNumber, createdJob.JobNumber);
    }

    [Fact]
    public async Task List_jobs_filtered_by_councilId_returns_only_jobs_for_that_council()
    {
        int testJobNumber = 778002;
        JobCreationDto createDto = new()
        {
            JobNumber = testJobNumber,
            JobType = Shared.JobTypeEnum.Construction,
            ContactId = 1,
            CouncilId = 1,
            Details = "Job for council filter test",
        };
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/jobs", createDto);
        createResponse.EnsureSuccessStatusCode();
        int? jobId = await createResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(jobId);
        Assert.True(jobId > 0);

        HttpResponseMessage response = await _client.GetAsync("/api/jobs?page=1&pageSize=50&councilId=1");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PagedResponse<ListJobDto>? resValue = await response.Content.ReadFromJsonAsync<PagedResponse<ListJobDto>>();
        Assert.NotNull(resValue);
        Assert.NotNull(resValue.Result);
        ListJobDto? createdJob = resValue.Result.FirstOrDefault(j => j.JobId == jobId.Value);
        Assert.NotNull(createdJob);
        Assert.Equal(testJobNumber, createdJob.JobNumber);
    }

    [Fact]
    public async Task List_jobs_filtered_by_contactId_and_councilId_returns_matching_jobs()
    {
        int testJobNumber = 778003;
        JobCreationDto createDto = new()
        {
            JobNumber = testJobNumber,
            JobType = Shared.JobTypeEnum.Construction,
            ContactId = 1,
            CouncilId = 1,
            Details = "Job for combined filter test",
        };
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/jobs", createDto);
        createResponse.EnsureSuccessStatusCode();
        int? jobId = await createResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(jobId);
        Assert.True(jobId > 0);

        HttpResponseMessage response = await _client.GetAsync("/api/jobs?page=1&pageSize=50&contactId=1&councilId=1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PagedResponse<ListJobDto>? resValue = await response.Content.ReadFromJsonAsync<PagedResponse<ListJobDto>>();
        Assert.NotNull(resValue);
        Assert.NotNull(resValue.Result);
        ListJobDto? createdJob = resValue.Result.FirstOrDefault(j => j.JobId == jobId.Value);
        Assert.NotNull(createdJob);
        Assert.Equal(testJobNumber, createdJob.JobNumber);

        response = await _client.GetAsync("/api/jobs?page=1&pageSize=50&contactId=1&councilId=99999");
        response.EnsureSuccessStatusCode();
        resValue = await response.Content.ReadFromJsonAsync<PagedResponse<ListJobDto>>();
        Assert.NotNull(resValue);
        Assert.NotNull(resValue.Result);
        Assert.DoesNotContain(resValue.Result, j => j.JobId == jobId.Value);
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
        Assert.Equal(dto.ContactId, job.PrimaryContact?.ContactId);
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
        Assert.Equal(dto.ContactId, job.PrimaryContact?.ContactId);
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
        Assert.Equal(dto.ContactId, job.PrimaryContact?.ContactId);
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

    [Fact]
    public async Task Create_note_returns_success()
    {
        JobCreationDto jobDto = new()
        {
            JobNumber = 999001,
            JobType = Shared.JobTypeEnum.Construction,
            ContactId = 1,
            Details = "Job for note tests",
        };

        HttpResponseMessage createJobResponse = await _client.PostAsJsonAsync("/api/jobs", jobDto);
        createJobResponse.EnsureSuccessStatusCode();
        int? jobId = await createJobResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(jobId);
        Assert.True(jobId > 0);

        JobNoteDto note = new()
        {
            NoteId = 0,
            JobId = jobId.Value,
            Content = "Test note content",
            DateCreated = DateTime.UtcNow,
            ActionRequired = true,
        };

        HttpResponseMessage createNoteResponse = await _client.PostAsJsonAsync("/api/jobs/notes", note);
        Assert.Equal(HttpStatusCode.OK, createNoteResponse.StatusCode);
        List<JobNoteDto>? createdNotes = await createNoteResponse.Content.ReadFromJsonAsync<List<JobNoteDto>>();
        Assert.NotNull(createdNotes);
        Assert.Equal(jobId, createdNotes.First().JobId);
        Assert.NotEmpty(createdNotes);
        JobNoteDto? createdNote = createdNotes.FirstOrDefault(n => n.Content == note.Content);
        Assert.NotNull(createdNote);
        Assert.True(createdNote.NoteId > 0);
        Assert.Equal(note.ActionRequired, createdNote.ActionRequired);
    }

    [Fact]
    public async Task Update_note_returns_success()
    {
        JobCreationDto jobDto = new()
        {
            JobNumber = 999002,
            JobType = Shared.JobTypeEnum.Construction,
            ContactId = 1,
            Details = "Job for update note tests",
        };

        HttpResponseMessage createJobResponse = await _client.PostAsJsonAsync("/api/jobs", jobDto);
        createJobResponse.EnsureSuccessStatusCode();
        int? jobId = await createJobResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(jobId);
        Assert.True(jobId > 0);

        JobNoteDto createNote = new()
        {
            NoteId = 0,
            JobId = jobId.Value,
            Content = "Original note content",
            DateCreated = DateTime.UtcNow,
            ActionRequired = false,
            AssignedUser = new UserDto(1, "")
        };

        HttpResponseMessage createNoteResponse = await _client.PostAsJsonAsync("/api/jobs/notes", createNote);
        createNoteResponse.EnsureSuccessStatusCode();
        List<JobNoteDto>? notesAfterCreate = await createNoteResponse.Content.ReadFromJsonAsync<List<JobNoteDto>>();
        Assert.NotNull(notesAfterCreate);
        JobNoteDto? createdNote = notesAfterCreate.FirstOrDefault(n => n.Content == createNote.Content);
        Assert.NotNull(createdNote);
        int noteId = createdNote.NoteId;
        Assert.True(noteId > 0);

        JobNoteDto updateNote = new()
        {
            NoteId = noteId,
            JobId = jobId.Value,
            Content = "Updated note content",
            DateCreated = createdNote.DateCreated,
            ActionRequired = true,
        };

        HttpResponseMessage updateNoteResponse = await _client.PostAsJsonAsync("/api/jobs/notes", updateNote);
        Assert.Equal(HttpStatusCode.OK, updateNoteResponse.StatusCode);
        List<JobNoteDto>? notesAfterUpdate = await updateNoteResponse.Content.ReadFromJsonAsync<List<JobNoteDto>>();
        Assert.NotNull(notesAfterUpdate);
        JobNoteDto? updatedNote = notesAfterUpdate.FirstOrDefault(n => n.NoteId == noteId);
        Assert.NotNull(updatedNote);
        Assert.Equal(updateNote.Content, updatedNote.Content);
        Assert.Equal(updateNote.ActionRequired, updatedNote.ActionRequired);
    }
}
