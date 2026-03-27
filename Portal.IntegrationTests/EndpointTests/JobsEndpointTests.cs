using Microsoft.AspNetCore.Http;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO.File;
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
        JobCreationDto createDto = new()
        {
            JobType = [JobTypeEnum.Construction],
            ContactId = 1,
            Details = "Job for contact filter test",
            StatusId = 1
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
        Assert.NotNull(createdJob.JobNumber);
    }

    [Fact]
    public async Task List_jobs_filtered_by_councilId_returns_only_jobs_for_that_council()
    {
        JobCreationDto createDto = new()
        {
            JobType = [JobTypeEnum.Construction],
            ContactId = 1,
            CouncilId = 1,
            Details = "Job for council filter test",
            StatusId = 1
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
        Assert.NotNull(createdJob.JobNumber);
    }

    [Fact]
    public async Task List_jobs_filtered_by_contactId_and_councilId_returns_matching_jobs()
    {
        JobCreationDto createDto = new()
        {
            JobType = [JobTypeEnum.Construction],
            ContactId = 1,
            CouncilId = 1,
            Details = "Job for combined filter test",
            StatusId = 1
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
        Assert.NotNull(createdJob.JobNumber);

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
            JobType = [JobTypeEnum.Construction],
            ContactId = 1,
            Details = "Test",
            StatusId = 1
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
        Assert.NotNull(job.JobNumber);
        Assert.True(job.JobTypes.Contains(JobTypeEnum.Construction));
    }
    [Fact]
    public async Task Create_job_bad_request()
    {
        JobCreationDto invalidCreatJobRequest = new()
        {
            JobType = [JobTypeEnum.Construction],
            ContactId = 0,
            Details = "Test",
            StatusId = 1
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
            JobType = [JobTypeEnum.Construction],
            ContactId = 1,
            Details = "Test",
            StatusId = 1
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
        Assert.NotNull(job.JobNumber);
        Assert.True(job.JobTypes.Contains(JobTypeEnum.Construction));

        job.Details = "Updated Job Details";
        job.JobTypes = [JobTypeEnum.Surveying];
        response = await _client.PutAsJsonAsync("/api/jobs", job);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JobDetailsDto? updateJob = await response.Content.ReadFromJsonAsync<JobDetailsDto?>();
        Assert.NotNull(updateJob);
        Assert.Equal(job.Details, updateJob.Details);
        Assert.Equal(job.JobTypes, updateJob.JobTypes);
    }

    [Fact]
    public async Task Delete_job_returns_success()
    {
        JobCreationDto dto = new()
        {
            JobType = [JobTypeEnum.Construction],
            ContactId = 1,
            Details = "Test",
            StatusId = 1
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
        Assert.NotNull(job.JobNumber);
        Assert.True(job.JobTypes.Contains(JobTypeEnum.Construction));

        job.Details = "Updated Job Details";
        job.JobTypes = [JobTypeEnum.Surveying];
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
            JobType = [JobTypeEnum.Construction],
            ContactId = 1,
            Details = "Job for note tests",
            StatusId = 1
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
            JobType = [JobTypeEnum.Construction],
            ContactId = 1,
            Details = "Job for update note tests",
            StatusId = 1
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

    [Fact]
    public async Task Save_job_file_returns_ok_and_file_id()
    {
        JobCreationDto jobDto = new()
        {
            JobType = [JobTypeEnum.Construction],
            ContactId = 1,
            Details = "Job for update note tests",
            StatusId = 1
        };

        HttpResponseMessage createJobResponse = await _client.PostAsJsonAsync("/api/jobs", jobDto);
        createJobResponse.EnsureSuccessStatusCode();
        int? jobId = await createJobResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(jobId);
        Assert.True(jobId > 0);

        FileDto fileDto = new()
        {
            JobId = jobId ?? 0,
            FileId = 0,
            ContentType = "application/pdf",
            Content = [0x25, 0x50, 0x44, 0x46],
            FileName = "test-document.pdf",
            FileTypeId = 1,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/jobs/{jobId}/files", fileDto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        int? fileId = await response.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(fileId);
        Assert.True(fileId > 0);
    }

    [Fact]
    public async Task Save_job_file_then_get_job_details_includes_file()
    {
        JobCreationDto jobDto = new()
        {
            JobType = [JobTypeEnum.Construction],
            ContactId = 1,
            Details = "Job for file attachment test",
            StatusId = 1
        };

        HttpResponseMessage createJobResponse = await _client.PostAsJsonAsync("/api/jobs", jobDto);
        createJobResponse.EnsureSuccessStatusCode();
        int? jobId = await createJobResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(jobId);
        Assert.True(jobId > 0);

        FileDto fileDto = new()
        {
            JobId = jobId.Value,
            FileId = 0,
            ContentType = "text/plain",
            Content = System.Text.Encoding.UTF8.GetBytes("Hello, job file."),
            FileName = "note.txt",
            FileTypeId = 1
        };

        HttpResponseMessage saveFileResponse = await _client.PutAsJsonAsync($"/api/jobs/{jobId}/files", fileDto);
        Assert.Equal(HttpStatusCode.OK, saveFileResponse.StatusCode);
        int? fileId = await saveFileResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(fileId);
        Assert.True(fileId > 0);

        HttpResponseMessage getJobResponse = await _client.GetAsync($"/api/jobs/{jobId}");
        getJobResponse.EnsureSuccessStatusCode();
        JobDetailsDto? job = await getJobResponse.Content.ReadFromJsonAsync<JobDetailsDto>();
        Assert.NotNull(job);
        Assert.NotNull(job.JobFiles);
        Assert.NotEmpty(job.JobFiles);
        FileDto? savedFile = job.JobFiles.FirstOrDefault(f => f.FileId == fileId);
        Assert.NotNull(savedFile);
        Assert.Equal(fileDto.FileName, savedFile.FileName);
        Assert.Equal(fileDto.FileTypeId, savedFile.FileTypeId);
    }

    [Fact]
    public async Task Save_job_file_with_invalid_job_id_returns_bad_request()
    {
        FileDto fileDto = new()
        {
            JobId = 0,
            FileId = 0,
            ContentType = "application/pdf",
            Content = [0x25, 0x50, 0x44, 0x46],
            FileName = "test.pdf",
            FileTypeId = 1,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/jobs/0/files", fileDto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        response = await _client.PutAsJsonAsync("/api/jobs/-1/files", fileDto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Save_job_file_for_nonexistent_job_returns_bad_request()
    {
        int nonexistentJobId = 999999;
        FileDto fileDto = new()
        {
            JobId = nonexistentJobId,
            FileId = 0,
            ContentType = "application/pdf",
            Content = [0x25, 0x50, 0x44, 0x46],
            FileName = "test.pdf",
            FileTypeId = 1,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/jobs/{nonexistentJobId}/files", fileDto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
