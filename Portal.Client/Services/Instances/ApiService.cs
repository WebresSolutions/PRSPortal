using Microsoft.AspNetCore.Components;
using Portal.Client.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Councils;
using Portal.Shared.DTO.File;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.DTO.Setting;
using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;
using System.Net.Http.Json;

namespace Portal.Client.Services.Instances;

/// <summary>
/// Service implementation for making API calls to the server
/// Handles HTTP requests with authentication and error handling
/// </summary>
public class ApiService : IApiService
{
    /// <summary>
    /// The HTTP client used for making API requests
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Navigation manager for handling page navigation
    /// </summary>
    private readonly NavigationManager _navigationManager;
    /// <summary>
    /// Initializes a new instance of the ApiService class
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="navigationManager">Navigation manager for redirecting to login</param>
    /// <exception cref="Exception">Thrown when HTTP client configuration is missing</exception>
    public ApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration, NavigationManager navigationManager)
    {
        string httpClientName = configuration.GetValue<string>("HttpClient")
            ?? throw new Exception("Failed to load the http client settings");

        _httpClient = httpClientFactory.CreateClient(httpClientName);
        _navigationManager = navigationManager;
    }

    /// <summary>
    /// Retrieves a paged list of jobs, optionally filtered by name and sorted according to the specified criteria.
    /// </summary>
    /// <remarks>If the request is unauthorized, the user may be redirected to the login page. The method does
    /// not throw exceptions for HTTP errors; instead, error information is included in the returned result.</remarks>
    /// <param name="pageSize">The maximum number of jobs to include in each page of results. Must be a positive integer.</param>
    /// <param name="pageNumber">The 1-based index of the page to retrieve. Must be greater than or equal to 1.</param>
    /// <param name="nameFilter">An optional filter to return only jobs whose names contain the specified value. If null, no name filtering is
    /// applied.</param>
    /// <param name="orderby">An optional field name by which to sort the results. If null, the default sort order is used.</param>
    /// <param name="order">The direction in which to sort the results. Specify ascending or descending.</param>
    /// <returns>A result containing a paged response of job data transfer objects. If no jobs match the criteria, the response
    /// contains an empty collection.</returns>
    public async Task<Result<PagedResponse<ListJobDto>>> GetAllJobs(JobFilterDto filter)
    {
        Result<PagedResponse<ListJobDto>> res = new();
        try
        {
            Dictionary<string, object?> queryParams = new()
            {
                ["page"] = filter.Page,
                ["pageSize"] = filter.PageSize,
                ["order"] = (int)filter.Order,
                ["deleted"] = filter.Deleted.ToString().ToLower(),
                ["addressSearch"] = filter.AddressSearch,
                ["contactSearch"] = filter.ContactSearch,
                ["jobNumberSearch"] = filter.JobNumberSearch,
                ["orderby"] = filter.OrderBy,
                ["councilId"] = filter.CouncilId,
                ["contactId"] = filter.ContactId
            };

            string url = _navigationManager.GetUriWithQueryParameters("api/jobs", queryParams);

            // 4. Execute the request
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await NavigationToLoginPage();
                return res.SetError(ErrorType.Unauthorized, "Unauthorized access");
            }

            if (response.IsSuccessStatusCode)
            {
                res.Value = await response.Content.ReadFromJsonAsync<PagedResponse<ListJobDto>>();
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get all of the jobs";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            res.SetError(ErrorType.InternalError, "A client-side error occurred.");
        }

        return res;
    }
    /// <summary>
    /// Retrieves the details of a job with the specified identifier.
    /// </summary>
    /// <remarks>If the request is unauthorized, the user may be redirected to the login page. The returned
    /// result will contain error information if the job is not found or if the request fails.</remarks>
    /// <param name="id">The unique identifier of the job to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{JobDetailsDto}"/> object with the job details if found; otherwise, contains error information.</returns>
    public async Task<Result<JobDetailsDto>> Job(int id)
    {
        Result<JobDetailsDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/jobs/{id}");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                JobDetailsDto? job = await response.Content.ReadFromJsonAsync<JobDetailsDto>();
                res.Value = job;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get system settings";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            // Handle exception
        }
        return res;
    }
    /// <summary>
    /// Creates a new job with the provided details.
    /// </summary>
    public async Task<Result<int>> CreateJob(JobCreationDto data)
    {
        Result<int> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/jobs", data);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                int? id = await response.Content.ReadFromJsonAsync<int>();
                if (id.HasValue)
                    res.Value = id.Value;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to create job";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <summary>
    /// Deletes a job with the specified identifier. If the user is not authorized, the method may trigger navigation to the login page. The returned result contains error information if the request fails.
    /// </summary>
    /// <param name="jobId"></param>
    /// <returns></returns>
    public async Task<Result<bool>> DeleteJob(int jobId)
    {
        Result<bool> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/jobs/{jobId}");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                bool? id = await response.Content.ReadFromJsonAsync<bool>();
                if (id.HasValue)
                    res.Value = id.Value;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to delete job";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <summary>
    /// Updates Job Details
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task<Result<JobDetailsDto>> UpdateJob(JobDetailsDto data)
    {
        Result<JobDetailsDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/jobs", data);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                JobDetailsDto? jobDetails = await response.Content.ReadFromJsonAsync<JobDetailsDto>();

                if (jobDetails is not null)
                    res.Value = jobDetails;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to update job";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <summary>
    /// Gets the notes for a job.
    /// </summary>
    /// <param name="jobId">The job id.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted notes.</param>
    /// <returns>The list of notes for the job.</returns>
    public async Task<Result<List<JobNoteDto>>> GetJobNotes(int jobId, bool includeDeleted = false, bool? actionRequired = null)
    {
        Result<List<JobNoteDto>> res = new();
        try
        {
            string url = $"api/jobs/{jobId}/notes?deleted={includeDeleted.ToString().ToLowerInvariant()}";
            if (actionRequired is not null)
                url += $"&actionRequired={actionRequired?.ToString().ToLowerInvariant()}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                List<JobNoteDto>? notes = await response.Content.ReadFromJsonAsync<List<JobNoteDto>>();
                res.Value = notes ?? [];
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get job notes";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <summary>
    /// Creates or updates a job note. Use NoteId 0 to create; set NoteId to the existing note id to update.
    /// </summary>
    /// <param name="note">The note to create or update.</param>
    /// <returns>The updated job details including notes.</returns>
    public async Task<Result<List<JobNoteDto>>> SaveJobNote(JobNoteDto note)
    {
        Result<List<JobNoteDto>> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/jobs/notes", note);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                List<JobNoteDto>? notes = await response.Content.ReadFromJsonAsync<List<JobNoteDto>>();
                if (notes is not null)
                    res.Value = notes;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to save job note";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <summary>
    /// Gets technical contacts filtered by jobId and/or contactId. At least one of jobId or contactId must be provided.
    /// </summary>
    public async Task<Result<TechnicalContactDto[]>> GetTechnicalContacts(int? jobId, int? contactId, bool showDeleted = false)
    {
        Result<TechnicalContactDto[]> res = new();
        try
        {
            Dictionary<string, object?> queryParams = [];
            if (jobId.HasValue) queryParams["jobId"] = jobId.Value;
            if (contactId.HasValue) queryParams["contactId"] = contactId.Value;
            queryParams["deleted"] = showDeleted.ToString().ToLowerInvariant();

            string url = _navigationManager.GetUriWithQueryParameters("api/jobs/technical-contacts", queryParams);
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                TechnicalContactDto[]? contacts = await response.Content.ReadFromJsonAsync<TechnicalContactDto[]>();
                res.Value = contacts ?? Array.Empty<TechnicalContactDto>();
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get technical contacts";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <summary>
    /// Creates or updates a technical contact (links a contact to a job with a role). Use Id 0 to create; set Id to the existing technical contact id to update.
    /// </summary>
    public async Task<Result<TechnicalContactDto[]>> SaveTechnicalContact(SaveTechnicalContactTypeDto dto)
    {
        Result<TechnicalContactDto[]> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/jobs/technical-contacts", dto);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                TechnicalContactDto[]? contacts = await response.Content.ReadFromJsonAsync<TechnicalContactDto[]>();
                if (contacts is not null)
                    res.Value = contacts;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to save technical contact";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }

    /// <summary>
    /// Saves (uploads or replaces) a file for a job.
    /// </summary>
    public async Task<Result<int>> SaveJobFile(int jobId, FileDto file)
    {
        Result<int> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/jobs/{jobId}/files", file);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                int? fileId = await response.Content.ReadFromJsonAsync<int?>();
                if (fileId.HasValue)
                    res.Value = fileId.Value;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to save job file";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }

    /// <summary>
    /// Gets file data (metadata and content) by file id.
    /// </summary>
    public async Task<Result<FileDto>> GetFileData(int fileId)
    {
        Result<FileDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/files/{fileId}");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                FileDto? dto = await response.Content.ReadFromJsonAsync<FileDto>();
                if (dto is not null)
                    res.Value = dto;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get file";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }

    /// <summary>
    /// Retrieves the list of available schedule slots for an individual on the specified date and job type.
    /// </summary>
    /// <remarks>If the user is not authorized, the operation will redirect to the login page. The returned
    /// Result object contains error information if the request fails.</remarks>
    /// <param name="date">The date for which to retrieve available schedule slots.</param>
    /// <param name="jobType">The type of job for which to retrieve schedule slots.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with a list of
    /// ScheduleSlotDTO instances representing the available schedule slots. If no slots are available, the list will be
    /// empty.</returns>
    public async Task<Result<List<ScheduleTrackDto>>> GetIndividualSchedule(DateOnly date, JobTypeEnum jobType)
    {
        Result<List<ScheduleTrackDto>> res = new();
        try
        {
            Dictionary<string, string> queryParameters = new()
            {
                { "date", date.ToString() },
                { "jobType", jobType.ToString() }
            };

            FormUrlEncodedContent dictFormUrlEncoded = new(queryParameters);
            string queryString = await dictFormUrlEncoded.ReadAsStringAsync();

            HttpResponseMessage response = await _httpClient.GetAsync($"api/schedule/slots/?{queryString}");

            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();

            if (response.IsSuccessStatusCode)
            {
                List<ScheduleTrackDto>? slots = await response.Content.ReadFromJsonAsync<List<ScheduleTrackDto>>();
                res.Value = slots;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get all of the jobs";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            // Handle exception
        }
        return res;
    }
    /// <summary>
    /// Retrieves the list of available schedule colours from the server.
    /// </summary>
    /// <remarks>If the user is not authorized, the method may trigger navigation to the login page before
    /// returning. The returned list may be empty if no schedule colours are available.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/> object
    /// with a list of <see cref="ScheduleColourDto"/> instances if the request is successful; otherwise, contains error
    /// information describing the failure.</returns>
    public async Task<Result<List<ScheduleColourDto>>> GetScheduleColours()
    {
        Result<ScheduleColourDto[]> arrayResult = await GetTypesAsync<ScheduleColourDto>("api/types/schedulecolour", "schedule colours");
        Result<List<ScheduleColourDto>> res = new();
        if (arrayResult.IsSuccess && arrayResult.Value is { } arr)
            res.SetValue(arr.ToList());
        else
            res.SetError(arrayResult.Error ?? ErrorType.InternalError, arrayResult.ErrorDescription ?? "Failed to get schedule colours");
        return res;
    }
    /// <summary>
    /// Updates the schedule colour using the specified colour data.
    /// </summary>
    /// <remarks>If the update request is unauthorized, the user is redirected to the login page. The returned
    /// Result object provides details about the success or failure of the operation.</remarks>
    /// <param name="colour">The schedule colour data to update. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the updated
    /// schedule colour data if the update is successful; otherwise, contains error information.</returns>
    public async Task<Result<ScheduleColourDto>> UpdateScheduleColour(ScheduleColourDto colour)
    {
        Result<ScheduleColourDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/schedule/colours/", colour);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();

            if (response.IsSuccessStatusCode)
            {
                ScheduleColourDto? colours = await response.Content.ReadFromJsonAsync<ScheduleColourDto>();
                res.Value = colours;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to save schedule colours";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            // Handle exception
        }
        return res;
    }

    /// <summary>
    /// Creates or updates a schedule. Use Id 0 to create; set Id to the existing schedule id to update.
    /// </summary>
    public async Task<Result<int>> UpdateSchedule(UpdateScheduleDto data)
    {
        Result<int> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/schedule", data);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                int? id = await response.Content.ReadFromJsonAsync<int>();
                if (id.HasValue)
                    res.Value = id.Value;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to save schedule";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }

    /// <summary>
    /// Creates or updates a schedule track (day slot). Use ScheduleTrackId 0 to create.
    /// </summary>
    public async Task<Result<ScheduleTrackDto>> UpdateScheduleTrack(UpdateScheduleTrackDto data)
    {
        Result<ScheduleTrackDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/schedule/tracks", data);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                ScheduleTrackDto? dto = await response.Content.ReadFromJsonAsync<ScheduleTrackDto>();
                if (dto is not null)
                    res.Value = dto;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to save schedule track";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }

    /// <summary>
    /// Gets the weekly schedule for the given job type, optionally for the week containing the specified day.
    /// </summary>
    public async Task<Result<WeeklyGroupedByScheduleDto[]>> GetWeeklySchedule(JobTypeEnum jobType, DateOnly? weekDay = null)
    {
        Result<WeeklyGroupedByScheduleDto[]> res = new();
        try
        {
            Dictionary<string, string> queryParams = new()
            {
                { "jobType", jobType.ToString() }
            };
            if (weekDay.HasValue)
                queryParams["weekDay"] = weekDay.Value.ToString("yyyy-MM-dd");

            FormUrlEncodedContent dictFormUrlEncoded = new(queryParams);
            string queryString = await dictFormUrlEncoded.ReadAsStringAsync();
            HttpResponseMessage response = await _httpClient.GetAsync($"api/schedule/week?{queryString}");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                WeeklyGroupedByScheduleDto[]? data = await response.Content.ReadFromJsonAsync<WeeklyGroupedByScheduleDto[]>();
                res.Value = data ?? [];
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get weekly schedule";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }

    /// <summary>
    /// Deletes a schedule track.
    /// </summary>
    public async Task<Result<int>> DeleteScheduleTrack(int id)
    {
        Result<int> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/schedule/tracks/{id}");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                int data = await response.Content.ReadFromJsonAsync<int>();
                res.Value = data;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get weekly schedule";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }

    /// <summary>
    /// Retrieves the current system settings from the server asynchronously.
    /// </summary>
    /// <remarks>If the user is not authorized, the method may trigger navigation to the login page. The
    /// returned result object includes error details if the operation fails, such as when the server returns an error
    /// response.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{SystemSettingDto}"/> object with the retrieved system settings if the request is successful;
    /// otherwise, contains error information.</returns>
    public async Task<Result<SystemSettingDto>> GetSystemSettings()
    {
        Result<SystemSettingDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/settings/systemsettings/");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                SystemSettingDto? settings = await response.Content.ReadFromJsonAsync<SystemSettingDto>();
                res.Value = settings;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get system settings";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            // Handle exception
        }
        return res;
    }
    /// <summary>
    /// Updates the system settings with the specified values asynchronously.
    /// </summary>
    /// <remarks>If the operation is unauthorized, the user may be redirected to the login page. The returned
    /// Result object provides details about the success or failure of the update operation.</remarks>
    /// <param name="settings">An object containing the new system settings to apply. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the updated
    /// system settings if the update is successful; otherwise, contains error information.</returns>
    public async Task<Result<SystemSettingDto>> UpdateSystemSettings(SystemSettingDto settings)
    {
        Result<SystemSettingDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/settings/systemsettings/", settings);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                SystemSettingDto? updatedSettings = await response.Content.ReadFromJsonAsync<SystemSettingDto>();
                res.Value = updatedSettings;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to save system settings";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            // Handle exception
        }
        return res;
    }
    /// <summary>
    /// Retrieves all councils from the server asynchronously.
    /// </summary>
    /// <remarks>If the user is not authorized, the method may trigger navigation to the login page. The returned
    /// result object includes error details if the operation fails.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{CouncilPartialDto[]}"/> object with the list of councils if the request is successful;
    /// otherwise, contains error information.</returns>
    public async Task<Result<CouncilPartialDto[]>> GetCouncils()
    {
        Result<CouncilPartialDto[]> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/councils");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                CouncilPartialDto[]? councils = await response.Content.ReadFromJsonAsync<CouncilPartialDto[]>();
                res.Value = councils;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get all councils";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            // Handle exception
        }
        return res;
    }
    /// <summary>
    /// Retrieves the details of a council with the specified identifier.
    /// </summary>
    /// <remarks>If the request is unauthorized, the user may be redirected to the login page. The returned
    /// result will contain error information if the council is not found or if the request fails.</remarks>
    /// <param name="councilId">The unique identifier of the council to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{CouncilDetailsDto}"/> object with the council details if found; otherwise, contains error information.</returns>
    public async Task<Result<CouncilDetailsDto>> GetCouncilDetails(int councilId)
    {
        Result<CouncilDetailsDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/councils/{councilId}");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                CouncilDetailsDto? council = await response.Content.ReadFromJsonAsync<CouncilDetailsDto>();
                res.Value = council;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get council details";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            // Handle exception
        }
        return res;
    }
    /// <summary>
    /// Retrieves a paged list of contacts, optionally filtered by split search fields or searchFilter for type-ahead, and sorted.
    /// </summary>
    /// <remarks>If the request is unauthorized, the user may be redirected to the login page. The method does
    /// not throw exceptions for HTTP errors; instead, error information is included in the returned result.</remarks>
    /// <param name="filter">Filter parameters including page, pageSize, order, orderby, deleted, and optional search fields or searchFilter.</param>
    /// <returns>A result containing a paged response of contact data transfer objects. If no contacts match the criteria, the response
    /// contains an empty collection.</returns>
    public async Task<Result<PagedResponse<ListContactDto>>> GetAllContacts(ContactFilterDto filter)
    {
        Result<PagedResponse<ListContactDto>> res = new();
        try
        {
            Dictionary<string, string> queryParameters = new()
            {
                { "page", filter.Page.ToString() },
                { "pageSize", filter.PageSize.ToString() },
                { "order", ((int)filter.Order).ToString() },
                { "deleted", filter.Deleted.ToString().ToLower() }
            };

            if (!string.IsNullOrWhiteSpace(filter.OrderBy))
                queryParameters.Add("orderby", filter.OrderBy);
            if (!string.IsNullOrWhiteSpace(filter.SearchFilter))
                queryParameters.Add("searchFilter", filter.SearchFilter);
            if (!string.IsNullOrWhiteSpace(filter.NameEmailPhoneSearch))
                queryParameters.Add("nameEmailPhoneSearch", filter.NameEmailPhoneSearch);
            if (!string.IsNullOrWhiteSpace(filter.AddressSearch))
                queryParameters.Add("addressSearch", filter.AddressSearch);

            FormUrlEncodedContent dictFormUrlEncoded = new(queryParameters);
            string queryString = await dictFormUrlEncoded.ReadAsStringAsync();

            HttpResponseMessage response = await _httpClient.GetAsync($"api/contacts/?{queryString}");

            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();

            if (response.IsSuccessStatusCode)
            {
                PagedResponse<ListContactDto>? contacts = await response.Content.ReadFromJsonAsync<PagedResponse<ListContactDto>>();
                res.Value = contacts;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get all contacts";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            // Handle exception
        }
        return res;
    }
    /// <summary>
    /// Retrieves the details of a contact with the specified identifier.
    /// </summary>
    /// <remarks>If the request is unauthorized, the user may be redirected to the login page. The returned
    /// result will contain error information if the contact is not found or if the request fails.</remarks>
    /// <param name="contactId">The unique identifier of the contact to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{ContactDetailsDto}"/> object with the contact details if found; otherwise, contains error information.</returns>
    public async Task<Result<ContactDetailsDto>> GetContactDetails(int contactId)
    {
        Result<ContactDetailsDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/contacts/{contactId}");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                ContactDetailsDto? contact = await response.Content.ReadFromJsonAsync<ContactDetailsDto>();
                res.Value = contact;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get contact details";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            // Handle exception
        }
        return res;
    }

    /// <summary>
    /// Creates a new contact with the provided details.
    /// </summary>
    public async Task<Result<int>> CreateContact(ContactCreationDto data)
    {
        Result<int> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/contacts", data);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                int? id = await response.Content.ReadFromJsonAsync<int>();
                if (id.HasValue)
                    res.Value = id.Value;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to create contact";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }

    /// <summary>
    /// Updates an existing contact with the provided details.
    /// </summary>
    public async Task<Result<ContactDetailsDto>> UpdateContact(ContactUpdateDto data)
    {
        Result<ContactDetailsDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/contacts", data);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                ContactDetailsDto? contact = await response.Content.ReadFromJsonAsync<ContactDetailsDto>();
                if (contact is not null)
                    res.Value = contact;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to update contact";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }

    /// <summary>
    /// Gets notes for a particular user. If the user is not authorized, the method may trigger navigation to the login page. The returned result contains error information if the request fails.
    /// </summary>
    /// <returns></returns>
    public async Task<Result<JobNoteDto[]>> GetUserNotes(bool includeDeleted = false, bool? actionRequired = null)
    {
        Result<JobNoteDto[]> res = new();
        try
        {
            string url = $"api/users/notes/0?deleted={includeDeleted.ToString().ToLowerInvariant()}";
            if (actionRequired is not null)
                url += $"&actionRequired={actionRequired?.ToString().ToLowerInvariant()}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();

            if (response.IsSuccessStatusCode)
            {
                JobNoteDto[]? jobs = await response.Content.ReadFromJsonAsync<JobNoteDto[]>();
                res.Value = jobs;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get contact jobs";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            // Handle exception
        }
        return res;
    }
    /// <summary>
    /// Gets a list of users
    /// </summary>
    /// <returns>An array of users</returns>
    public async Task<Result<UserDto[]>> GetUsersList()
    {
        Result<UserDto[]> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/users");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();

            if (response.IsSuccessStatusCode)
            {
                UserDto[]? jobs = await response.Content.ReadFromJsonAsync<UserDto[]>();
                res.Value = jobs;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get contact jobs";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            // Handle exception
        }
        return res;
    }
    /// <summary>
    /// Gets timesheet entries for a user within a date range. Use userId 0 for the current user.
    /// </summary>
    public async Task<Result<TimeSheetDto[]>> GetUserTimeSheets(int userId, DateTime start, DateTime? end)
    {
        Result<TimeSheetDto[]> res = new();
        try
        {
            string query = $"?start={start:O}";
            if (end.HasValue)
                query += $"&end={end:O}";
            HttpResponseMessage response = await _httpClient.GetAsync($"api/timesheet/{userId}{query}");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                TimeSheetDto[]? data = await response.Content.ReadFromJsonAsync<TimeSheetDto[]>();
                res.Value = data ?? Array.Empty<TimeSheetDto>();
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get timesheets";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <summary>
    /// Adds a new timesheet entry for the current user.
    /// </summary>
    public async Task<Result<TimeSheetDto>> AddTimeSheetEntry(TimeSheetDto entry)
    {
        Result<TimeSheetDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/timesheet", entry);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                TimeSheetDto? data = await response.Content.ReadFromJsonAsync<TimeSheetDto>();
                res.Value = data!;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to add timesheet entry";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <summary>
    /// Updates a timesheet entry for the current user. The entry must have a valid ID corresponding to an existing timesheet entry. 
    /// If the entry does not exist or the update fails, the returned result will contain error information.
    /// </summary>
    /// <param name="entry">The timesheet entry being updated</param>
    /// <returns></returns>
    public async Task<Result<TimeSheetDto>> UpdateTimeSheet(TimeSheetDto entry)
    {
        Result<TimeSheetDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/timesheet", entry);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                TimeSheetDto? data = await response.Content.ReadFromJsonAsync<TimeSheetDto>();
                res.Value = data!;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to add timesheet entry";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <summary>
    /// Gets the list of timesheet entry types.
    /// </summary>
    public Task<Result<TimeTypeDto[]>> GetTimeSheetTypes() =>
        GetTypesAsync<TimeTypeDto>("api/types/timesheet", "timesheet types");
    public Task<Result<ContactTypeDto[]>> GetContactTypes() =>
        GetTypesAsync<ContactTypeDto>("api/types/contact", "contact types");
    public Task<Result<JobTypeDto[]>> GetJobTypes() =>
        GetTypesAsync<JobTypeDto>("api/types/job", "job types");
    public Task<Result<JobColourDto[]>> GetJobColours() =>
        GetTypesAsync<JobColourDto>("api/types/jobcolour", "job colours");
    public Task<Result<FileTypeDto[]>> GetFileTypes() =>
        GetTypesAsync<FileTypeDto>("api/types/file", "file types");
    public Task<Result<JobTaskTypeDto[]>> GetJobTaskTypes() =>
        GetTypesAsync<JobTaskTypeDto>("api/types/jobtask", "job task types");
    public Task<Result<TechnicalContactTypeDto[]>> GetTechnicalContactTypes() =>
        GetTypesAsync<TechnicalContactTypeDto>("api/types/technicalcontact", "technical contact types");
    public Task<Result<StateDto[]>> GetStates() =>
        GetTypesAsync<StateDto>("api/types/state", "states");
    private async Task<Result<T[]>> GetTypesAsync<T>(string url, string typeName)
    {
        Result<T[]> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                T[]? data = await response.Content.ReadFromJsonAsync<T[]>();
                res.Value = data ?? Array.Empty<T>();
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? $"Failed to get {typeName}";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    private async Task<Result<T>> PutTypeAsync<T>(string url, T dto, string typeName)
    {
        Result<T> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync(url, dto);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                T? data = await response.Content.ReadFromJsonAsync<T>();
                if (data is not null)
                    res.Value = data;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? $"Failed to save {typeName}";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    public Task<Result<TimeTypeDto>> SaveTimeSheetType(TimeTypeDto dto) =>
        PutTypeAsync("api/types/timesheet", dto, "timesheet type");
    public Task<Result<ContactTypeDto>> SaveContactType(ContactTypeDto dto) =>
        PutTypeAsync("api/types/contact", dto, "contact type");
    public Task<Result<JobTypeDto>> SaveJobType(JobTypeDto dto) =>
        PutTypeAsync("api/types/job", dto, "job type");
    public Task<Result<JobColourDto>> SaveJobColour(JobColourDto dto) =>
        PutTypeAsync("api/types/jobcolour", dto, "job colour");
    public Task<Result<FileTypeDto>> SaveFileType(FileTypeDto dto) =>
        PutTypeAsync("api/types/file", dto, "file type");
    public Task<Result<JobTaskTypeDto>> SaveJobTaskType(JobTaskTypeDto dto) =>
        PutTypeAsync("api/types/jobtask", dto, "job task type");
    public Task<Result<TechnicalContactTypeDto>> SaveTechnicalContactType(TechnicalContactTypeDto dto) =>
        PutTypeAsync("api/types/technicalcontact", dto, "technical contact type");

    /// <summary>
    /// Deletes a timesheet entry for the current user. The entry must have a valid ID corresponding to an existing timesheet entry. 
    /// If the entry does not exist or the update fails, the returned result will contain error information.
    /// </summary>
    /// <param name="entry">The timesheet entry being updated</param>
    /// <returns></returns>
    public async Task<Result<bool>> DeleteTimeSheetEntry(TimeSheetDto entry)
    {
        Result<bool> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/timesheet/{entry.Id}");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                bool? data = await response.Content.ReadFromJsonAsync<bool>();
                res.Value = data ?? false;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to add timesheet entry";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }

    /// <summary>
    /// Navigates asynchronously to the login page, preserving the current URL as the return destination after
    /// authentication.
    /// </summary>
    /// <remarks>This method forces a full page reload when redirecting to the login page. The current URL is
    /// included as the return URL parameter, allowing users to be redirected back after successful login.</remarks>
    /// <returns>A task that represents the asynchronous navigation operation.</returns>
    private async Task NavigationToLoginPage()
    {
        string returnUrl = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
        string loginUrl = $"{_navigationManager.BaseUri}Authentication/Login?returnUrl={returnUrl}";
        //_navigationManager.NavigateTo(loginUrl, true);
    }

}
