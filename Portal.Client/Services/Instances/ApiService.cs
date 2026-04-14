using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Portal.Client.Services.Interfaces;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Councils;
using Portal.Shared.DTO.File;
using Portal.Shared.DTO.Integration;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Quote;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.DTO.Setting;
using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.DTO.Types;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;
using System.Globalization;
using System.Net.Http.Json;

namespace Portal.Client.Services.Instances;

/// <inheritdoc />
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    private readonly NavigationManager _navigationManager;

    public ApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration, NavigationManager navigationManager)
    {
        string httpClientName = configuration.GetValue<string>("HttpClient")
            ?? throw new Exception("Failed to load the http client settings");

        _httpClient = httpClientFactory.CreateClient(httpClientName);
        _navigationManager = navigationManager;
    }

    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
    public async Task<Result<JobDetailsDto>> UpdateJob(JobUpdateDto data)
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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
    public async Task<Result<List<ScheduleTrackDto>>> GetIndividualSchedule(DateOnly date, JobTypeEnum jobType)
    {
        Result<List<ScheduleTrackDto>> res = new();
        try
        {
            Dictionary<string, string> queryParameters = new()
            {
                // ISO 8601 date only — culture-invariant so the API can bind DateOnly (dd/MM fails under invariant/US parsing).
                { "date", date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
    public async Task<Result<ScheduleDto>> GetSchedule(int id)
    {
        Result<ScheduleDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/schedule/{id}");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                ScheduleDto? dto = await response.Content.ReadFromJsonAsync<ScheduleDto>();
                if (dto is not null)
                    res.Value = dto;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to load schedule";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
                queryParams["weekDay"] = weekDay.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
        }
        return res;
    }
    /// <inheritdoc />
    public async Task<Result<int>> CreateCouncil(CouncilCreationDto data)
    {
        Result<int> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/councils", data);
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
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to create council";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
    public async Task<Result<CouncilDetailsDto>> UpdateCouncil(CouncilUpdateDto data)
    {
        Result<CouncilDetailsDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/councils", data);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                CouncilDetailsDto? council = await response.Content.ReadFromJsonAsync<CouncilDetailsDto>();
                if (council is not null)
                    res.Value = council;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to update council";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
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
            if (filter.contactType is not null)
                queryParameters.Add("contactType", ((int)filter.contactType).ToString());

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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
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

    #region QUOTES
    /// <inheritdoc />
    public async Task<Result<PagedResponse<QuoteListDto>>> GetAllQuotes(QuoteFilterDto filter)
    {
        Result<PagedResponse<QuoteListDto>> res = new();
        try
        {
            Dictionary<string, object?> queryParams = new()
            {
                ["page"] = filter.Page,
                ["pageSize"] = filter.PageSize,
                ["order"] = (int)filter.Order,
                ["deleted"] = filter.Deleted.ToString().ToLower(),
                ["jobNumberSearch"] = filter.JobNumberSearch,
                ["contactSearch"] = filter.ContactSearch,
                ["addressSearch"] = filter.AddressSearch,
                ["orderby"] = filter.OrderBy
            };

            string url = _navigationManager.GetUriWithQueryParameters("api/quotes", queryParams);
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await NavigationToLoginPage();
                return res.SetError(ErrorType.Unauthorized, "Unauthorized access");
            }

            if (response.IsSuccessStatusCode)
            {
                res.Value = await response.Content.ReadFromJsonAsync<PagedResponse<QuoteListDto>>();
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get quotes";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            res.SetError(ErrorType.InternalError, "A client-side error occurred.");
        }

        return res;
    }
    /// <inheritdoc />
    public async Task<Result<QuoteDetailsDto>> GetQuoteDetails(int quoteId)
    {
        Result<QuoteDetailsDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/quotes/{quoteId}");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                QuoteDetailsDto? dto = await response.Content.ReadFromJsonAsync<QuoteDetailsDto>();
                if (dto is not null)
                    res.Value = dto;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get quote details";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
    public async Task<Result<QuotePdfDto>> GetQuotePdf(int quoteId)
    {
        Result<QuotePdfDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/quotes/{quoteId}/pdf");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                QuotePdfDto? dto = await response.Content.ReadFromJsonAsync<QuotePdfDto>();
                if (dto is not null)
                    res.Value = dto;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get quote pdf details";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
    public async Task<Result<int>> CreateQuote(QuoteCreationDto data)
    {
        Result<int> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/quotes", data);
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
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to create quote";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
    public async Task<Result<int>> UpdateQuote(QuoteUpdateDto data)
    {
        Result<int> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/quotes", data);
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
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to update quote";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
    public async Task<Result<bool>> DeleteQuote(int quoteId)
    {
        Result<bool> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/quotes/{quoteId}");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                bool? ok = await response.Content.ReadFromJsonAsync<bool>();
                if (ok.HasValue)
                    res.Value = ok.Value;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to delete quote";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
    public async Task<Result<QuoteTemplateDto[]>> GetQuotingTemplates()
    {
        Result<QuoteTemplateDto[]> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("api/quotes/templates");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                QuoteTemplateDto[]? templates = await response.Content.ReadFromJsonAsync<QuoteTemplateDto[]>();
                res.Value = templates ?? Array.Empty<QuoteTemplateDto>();
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get quoting templates";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
    public async Task<Result<QuoteTemplateDto>> CreateQuotingTemplate(QuoteTemplateDto data)
    {
        Result<QuoteTemplateDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/quotes/templates", data);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                QuoteTemplateDto? dto = await response.Content.ReadFromJsonAsync<QuoteTemplateDto>();
                if (dto is not null)
                    res.Value = dto;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to create quoting template";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
    public async Task<Result<QuoteTemplateDto>> UpdateQuotingTemplate(QuoteTemplateDto data)
    {
        Result<QuoteTemplateDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/quotes/templates", data);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                QuoteTemplateDto? dto = await response.Content.ReadFromJsonAsync<QuoteTemplateDto>();
                if (dto is not null)
                    res.Value = dto;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to update quoting template";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
    public async Task<Result<bool>> DeleteQuotingTemplate(int templateId)
    {
        Result<bool> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/quotes/templates/{templateId}");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                bool? ok = await response.Content.ReadFromJsonAsync<bool>();
                if (ok.HasValue)
                    res.Value = ok.Value;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to delete quoting template";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
    public async Task<Result<int>> SendQuoteToCustomer(int quoteId)
    {
        Result<int> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsync($"api/quotes/{quoteId}/send", null);
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
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to send quote to customer";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    #endregion
    /// <inheritdoc />
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
    /// <inheritdoc />
    public async Task<Result<UserJobsListDto>> GetUserJobs(int userId)
    {
        Result<UserJobsListDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/users/jobs/{userId}");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                UserJobsListDto? dto = await response.Content.ReadFromJsonAsync<UserJobsListDto>();
                if (dto is not null)
                    res.Value = dto;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get user jobs";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
    public Task<Result<TimeTypeDto[]>> GetTimeSheetTypes() =>
        GetTypesAsync<TimeTypeDto>("api/types/timesheet", "timesheet types");
    /// <inheritdoc />
    public Task<Result<ContactTypeDto[]>> GetContactTypes() =>
        GetTypesAsync<ContactTypeDto>("api/types/contact", "contact types");
    /// <inheritdoc />
    public Task<Result<JobTypeDto[]>> GetJobTypes() =>
        GetTypesAsync<JobTypeDto>("api/types/job", "job types");
    /// <inheritdoc />
    public Task<Result<JobColourDto[]>> GetJobColours() =>
        GetTypesAsync<JobColourDto>("api/types/jobcolour", "job colours");
    /// <inheritdoc />
    public Task<Result<FileTypeDto[]>> GetFileTypes() =>
        GetTypesAsync<FileTypeDto>("api/types/file", "file types");
    /// <inheritdoc />
    public Task<Result<JobTaskTypeDto[]>> GetJobTaskTypes() =>
        GetTypesAsync<JobTaskTypeDto>("api/types/jobtask", "job task types");
    /// <inheritdoc />
    public Task<Result<TechnicalContactTypeDto[]>> GetTechnicalContactTypes() =>
        GetTypesAsync<TechnicalContactTypeDto>("api/types/technicalcontact", "technical contact types");
    /// <inheritdoc />
    public Task<Result<StateDto[]>> GetStates() =>
        GetTypesAsync<StateDto>("api/types/state", "states");
    /// <inheritdoc />
    public async Task<Result<AllSettingsTypesDto>> GetAllSettingsTypes()
    {
        Result<AllSettingsTypesDto> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("api/types/all");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                AllSettingsTypesDto? data = await response.Content.ReadFromJsonAsync<AllSettingsTypesDto>();
                res.SetValue(data ?? new AllSettingsTypesDto());
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to load settings types";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /// <inheritdoc />
    public Task<Result<TimeTypeDto>> SaveTimeSheetType(TimeTypeDto dto) =>
        PutTypeAsync("api/types/timesheet", dto, "timesheet type");
    /// <inheritdoc />
    public Task<Result<ContactTypeDto>> SaveContactType(ContactTypeDto dto) =>
        PutTypeAsync("api/types/contact", dto, "contact type");
    /// <inheritdoc />
    public Task<Result<JobTypeDto>> SaveJobType(JobTypeDto dto) =>
        PutTypeAsync("api/types/job", dto, "job type");
    /// <inheritdoc />
    public Task<Result<JobColourDto>> SaveJobColour(JobColourDto dto) =>
        PutTypeAsync("api/types/jobcolour", dto, "job colour");
    /// <inheritdoc />
    public Task<Result<FileTypeDto>> SaveFileType(FileTypeDto dto) =>
        PutTypeAsync("api/types/file", dto, "file type");
    /// <inheritdoc />
    public Task<Result<JobTaskTypeDto>> SaveJobTaskType(JobTaskTypeDto dto) =>
        PutTypeAsync("api/types/jobtask", dto, "job task type");
    /// <inheritdoc />
    public Task<Result<TechnicalContactTypeDto>> SaveTechnicalContactType(TechnicalContactTypeDto dto) =>
        PutTypeAsync("api/types/technicalcontact", dto, "technical contact type");
    /// <inheritdoc />
    public Task<Result<ServiceTypeDto>> SaveServiceType(ServiceTypeDto dto) =>
        PutTypeAsync("api/types/service", dto, "service type");
    /// <inheritdoc />
    public async Task<Result<JobTypeStatusDto[]>> GetJobStatuses()
    {
        Result<JobTypeStatusDto[]> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("api/types/jobstatus");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                JobTypeStatusDto[]? data = await response.Content.ReadFromJsonAsync<JobTypeStatusDto[]>();
                res.SetValue(data ?? []);
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get job statuses";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
    public async Task<Result<JobTypeStatusDto[]>> SaveJobTypeStatuses(IEnumerable<JobTypeStatusDto> dtos)
    {
        Result<JobTypeStatusDto[]> res = new();
        try
        {
            JobTypeStatusDto[] body = dtos is JobTypeStatusDto[] arr ? arr : dtos.ToArray();
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/types/jobstatus", body);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                JobTypeStatusDto[]? data = await response.Content.ReadFromJsonAsync<JobTypeStatusDto[]>();
                if (data is not null)
                    res.SetValue(data);
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to save job statuses";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    #region INTEGRATION
    /// <inheritdoc />
    public async Task<Result<XeroAuthorizeResponse>> GetXeroAuthorizeUrl()
    {
        Result<XeroAuthorizeResponse> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("api/xero/authorize");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                XeroAuthorizeResponse? data = await response.Content.ReadFromJsonAsync<XeroAuthorizeResponse>();
                if (data is not null)
                    res.Value = data;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get Xero authorize URL";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
    public async Task<Result<XeroStatusResponse>> GetXeroStatus()
    {
        Result<XeroStatusResponse> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("api/xero/status");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                XeroStatusResponse? data = await response.Content.ReadFromJsonAsync<XeroStatusResponse>();
                if (data is not null)
                    res.Value = data;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get Xero status";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    /// <inheritdoc />
    public async Task<Result<bool>> DisconnectXero()
    {
        Result<bool> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync("api/xero/disconnect", null);
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            res.Value = response.IsSuccessStatusCode;
            if (!response.IsSuccessStatusCode)
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to disconnect Xero";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        return res;
    }
    #endregion

    /// <inheritdoc />
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
    private Task NavigationToLoginPage()
    {
        string returnPath = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
        if (string.IsNullOrEmpty(returnPath))
            returnPath = "/";
        else if (!returnPath.StartsWith('/'))
            returnPath = "/" + returnPath;

        InteractiveRequestOptions loginRequest = new()
        {
            Interaction = InteractionType.SignIn,
            ReturnUrl = returnPath
        };
        _navigationManager.NavigateToLogin("authentication/login", loginRequest);
        return Task.CompletedTask;
    }


}
