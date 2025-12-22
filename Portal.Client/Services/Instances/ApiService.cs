using Microsoft.AspNetCore.Components;
using Portal.Client.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.ResponseModels;
using System.Net.Http.Json;

namespace Portal.Client.Services.Instances;

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

    private async Task NavigationToLoginPage()
    {
        string returnUrl = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
        string loginUrl = $"{_navigationManager.BaseUri}Identity/Account/Login?returnUrl={returnUrl}";
        _navigationManager.NavigateTo(loginUrl, true);
    }
}
