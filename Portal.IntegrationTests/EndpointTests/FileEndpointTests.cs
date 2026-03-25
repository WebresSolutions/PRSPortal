using Portal.Shared.DTO.File;
using Portal.Shared.DTO.Job;
using System.Net;
using System.Net.Http.Json;

namespace Portal.IntegrationTests.EndpointTests;

[Collection(nameof(IntegrationTestCollection))]
public sealed class FileEndpointTests
{
    private readonly HttpClient _client;

    public FileEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Get_file_returns_ok_and_file_data_after_creating_via_job()
    {
        // Create a job
        JobCreationDto jobDto = new()
        {
            JobType = [Shared.JobTypeEnum.Construction],
            ContactId = 1,
            Details = "Job for get file test",
            StatusId = 1
        };

        HttpResponseMessage createJobResponse = await _client.PostAsJsonAsync("/api/jobs", jobDto);
        createJobResponse.EnsureSuccessStatusCode();
        int? jobId = await createJobResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(jobId);
        Assert.True(jobId > 0);

        // Save a file to the job
        byte[] fileContent = System.Text.Encoding.UTF8.GetBytes("Get file test content");
        FileDto fileDto = new()
        {
            JobId = jobId.Value,
            FileId = 0,
            ContentType = "text/plain",
            Content = fileContent,
            FileName = "get-file-test.txt",
            FileTypeId = 1,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
        };

        HttpResponseMessage saveFileResponse = await _client.PutAsJsonAsync($"/api/jobs/{jobId}/files", fileDto);
        Assert.Equal(HttpStatusCode.OK, saveFileResponse.StatusCode);
        int? fileId = await saveFileResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(fileId);
        Assert.True(fileId > 0);

        // Get file data via file endpoint
        HttpResponseMessage getFileResponse = await _client.GetAsync($"/api/files/{fileId}");
        Assert.Equal(HttpStatusCode.OK, getFileResponse.StatusCode);

        FileDto? retrieved = await getFileResponse.Content.ReadFromJsonAsync<FileDto>();
        Assert.NotNull(retrieved);
        Assert.Equal(fileId, retrieved.FileId);
        Assert.Equal(fileDto.FileName, retrieved.FileName);
        Assert.Equal(fileDto.FileTypeId, retrieved.FileTypeId);
        Assert.NotNull(retrieved.Content);
        Assert.True(retrieved.Content.Length > 0);
    }

    [Fact]
    public async Task Get_file_with_invalid_id_returns_bad_request()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/files/0");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_file_with_nonexistent_id_returns_bad_request()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/files/999999");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
