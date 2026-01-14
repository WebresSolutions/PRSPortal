using Microsoft.AspNetCore.Components;
using Portal.Client.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Councils;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.DTO.Setting;
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
    public async Task<Result<PagedResponse<ListJobDto>>> GetAllJobs(int pageSize, int pageNumber, string? nameFilter, string? orderby, SortDirectionEnum order)
    {
        Result<PagedResponse<ListJobDto>> res = new();
        try
        {
            Dictionary<string, string> queryParameters = new()
            {
                { "pageSize", pageSize.ToString() },
                { "page", pageNumber.ToString() },
                { "order", ((int)order).ToString() }
            };

            if (nameFilter is not null)
                queryParameters.Add("nameFilter", nameFilter ?? string.Empty);


            if (orderby is not null)
                queryParameters.Add("orderby", orderby ?? string.Empty);

            FormUrlEncodedContent dictFormUrlEncoded = new(queryParameters);
            string queryString = await dictFormUrlEncoded.ReadAsStringAsync();

            HttpResponseMessage response = await _httpClient.GetAsync($"api/jobs/?{queryString}");

            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();

            if (response.IsSuccessStatusCode)
            {
                PagedResponse<ListJobDto>? jobs = await response.Content.ReadFromJsonAsync<PagedResponse<ListJobDto>>();
                res.Value = jobs;
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
    /// Retrieves the list of available schedule slots for an individual on the specified date and job type.
    /// </summary>
    /// <remarks>If the user is not authorized, the operation will redirect to the login page. The returned
    /// Result object contains error information if the request fails.</remarks>
    /// <param name="date">The date for which to retrieve available schedule slots.</param>
    /// <param name="jobType">The type of job for which to retrieve schedule slots.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with a list of
    /// ScheduleSlotDTO instances representing the available schedule slots. If no slots are available, the list will be
    /// empty.</returns>
    public async Task<Result<List<ScheduleSlotDTO>>> GetIndividualSchedule(DateOnly date, JobTypeEnum jobType)
    {
        Result<List<ScheduleSlotDTO>> res = new();
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
                List<ScheduleSlotDTO>? slots = await response.Content.ReadFromJsonAsync<List<ScheduleSlotDTO>>();
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
        Result<List<ScheduleColourDto>> res = new();
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/schedule/colours/");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                List<ScheduleColourDto>? colours = await response.Content.ReadFromJsonAsync<List<ScheduleColourDto>>();
                res.Value = colours;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get schedule colours";
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
    /// Retrieves the jobs associated with a council with pagination.
    /// </summary>
    /// <remarks>If the request is unauthorized, the user may be redirected to the login page. The returned
    /// result will contain error information if the request fails.</remarks>
    /// <param name="councilId">The unique identifier of the council.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="order">The sort direction (ascending or descending).</param>
    /// <param name="orderby">Optional field name to sort by.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{PagedResponse{ListJobDto}}"/> object with the paged list of jobs if found; otherwise, contains error information.</returns>
    public async Task<Result<PagedResponse<ListJobDto>>> GetCouncilJobs(int councilId, int page, int pageSize, SortDirectionEnum order, string? orderby)
    {
        Result<PagedResponse<ListJobDto>> res = new();
        try
        {
            Dictionary<string, string> queryParameters = new()
            {
                { "page", page.ToString() },
                { "pageSize", pageSize.ToString() },
                { "order", ((int)order).ToString() }
            };

            if (orderby is not null)
                queryParameters.Add("orderby", orderby ?? string.Empty);

            FormUrlEncodedContent dictFormUrlEncoded = new(queryParameters);
            string queryString = await dictFormUrlEncoded.ReadAsStringAsync();

            HttpResponseMessage response = await _httpClient.GetAsync($"api/councils/{councilId}/jobs?{queryString}");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized)
                await NavigationToLoginPage();
            if (response.IsSuccessStatusCode)
            {
                PagedResponse<ListJobDto>? jobs = await response.Content.ReadFromJsonAsync<PagedResponse<ListJobDto>>();
                res.Value = jobs;
            }
            else
            {
                res.ConvertHttpResponseToError(response.StatusCode);
                res.ErrorDescription = await response.Content.ReadAsStringAsync() ?? "Failed to get council jobs";
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
    /// Navigates asynchronously to the login page, preserving the current URL as the return destination after
    /// authentication.
    /// </summary>
    /// <remarks>This method forces a full page reload when redirecting to the login page. The current URL is
    /// included as the return URL parameter, allowing users to be redirected back after successful login.</remarks>
    /// <returns>A task that represents the asynchronous navigation operation.</returns>
    private async Task NavigationToLoginPage()
    {
        string returnUrl = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
        string loginUrl = $"{_navigationManager.BaseUri}Identity/Account/Login?returnUrl={returnUrl}";
        _navigationManager.NavigateTo(loginUrl, true);
    }
}
