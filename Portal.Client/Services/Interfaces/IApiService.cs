using Portal.Shared;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Councils;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.DTO.Setting;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Services.Interfaces;

public interface IApiService
{
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
    Task<Result<PagedResponse<ListJobDto>>> GetAllJobs(int pageSize, int pageNumber, string? nameFilter, string? orderby, SortDirectionEnum order);
    /// <summary>
    /// Retrieves the details of a job with the specified identifier.
    /// </summary>
    /// <remarks>If the request is unauthorized, the user may be redirected to the login page. The returned
    /// result will contain error information if the job is not found or if the request fails.</remarks>
    /// <param name="id">The unique identifier of the job to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{JobDetailsDto}"/> object with the job details if found; otherwise, contains error information.</returns>
    Task<Result<JobDetailsDto>> Job(int id);
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
    Task<Result<List<ScheduleSlotDTO>>> GetIndividualSchedule(DateOnly date, JobTypeEnum jobType);
    /// <summary>
    /// Retrieves the list of available schedule colours from the server.
    /// </summary>
    /// <remarks>If the user is not authorized, the method may trigger navigation to the login page before
    /// returning. The returned list may be empty if no schedule colours are available.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/> object
    /// with a list of <see cref="ScheduleColourDto"/> instances if the request is successful; otherwise, contains error
    /// information describing the failure.</returns>
    Task<Result<List<ScheduleColourDto>>> GetScheduleColours();
    /// <summary>
    /// Updates the schedule colour using the specified colour data.
    /// </summary>
    /// <remarks>If the update request is unauthorized, the user is redirected to the login page. The returned
    /// Result object provides details about the success or failure of the operation.</remarks>
    /// <param name="colour">The schedule colour data to update. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the updated
    /// schedule colour data if the update is successful; otherwise, contains error information.</returns>
    Task<Result<SystemSettingDto>> GetSystemSettings();
    /// <summary>
    /// Retrieves the current system settings from the server asynchronously.
    /// </summary>
    /// <remarks>If the user is not authorized, the method may trigger navigation to the login page. The
    /// returned result object includes error details if the operation fails, such as when the server returns an error
    /// response.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{SystemSettingDto}"/> object with the retrieved system settings if the request is successful;
    /// otherwise, contains error information.</returns>
    Task<Result<ScheduleColourDto>> UpdateScheduleColour(ScheduleColourDto colour);
    /// <summary>
    /// Updates the system settings with the specified values asynchronously.
    /// </summary>
    /// <remarks>If the operation is unauthorized, the user may be redirected to the login page. The returned
    /// Result object provides details about the success or failure of the update operation.</remarks>
    /// <param name="settings">An object containing the new system settings to apply. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the updated
    /// system settings if the update is successful; otherwise, contains error information.</returns>
    Task<Result<SystemSettingDto>> UpdateSystemSettings(SystemSettingDto settings);
    /// <summary>
    /// Retrieves all councils from the server asynchronously.
    /// </summary>
    /// <remarks>If the user is not authorized, the method may trigger navigation to the login page. The returned
    /// result object includes error details if the operation fails.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{CouncilPartialDto[]}"/> object with the list of councils if the request is successful;
    /// otherwise, contains error information.</returns>
    Task<Result<CouncilPartialDto[]>> GetCouncils();
    /// <summary>
    /// Retrieves the details of a council with the specified identifier.
    /// </summary>
    /// <remarks>If the request is unauthorized, the user may be redirected to the login page. The returned
    /// result will contain error information if the council is not found or if the request fails.</remarks>
    /// <param name="councilId">The unique identifier of the council to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{CouncilDetailsDto}"/> object with the council details if found; otherwise, contains error information.</returns>
    Task<Result<CouncilDetailsDto>> GetCouncilDetails(int councilId);
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
    Task<Result<PagedResponse<ListJobDto>>> GetCouncilJobs(int councilId, int page, int pageSize, SortDirectionEnum order, string? orderby);
    /// <summary>
    /// Retrieves a paged list of contacts, optionally filtered by search term and sorted according to the specified criteria.
    /// </summary>
    /// <remarks>If the request is unauthorized, the user may be redirected to the login page. The method does
    /// not throw exceptions for HTTP errors; instead, error information is included in the returned result.</remarks>
    /// <param name="pageSize">The maximum number of contacts to include in each page of results. Must be a positive integer.</param>
    /// <param name="pageNumber">The 1-based index of the page to retrieve. Must be greater than or equal to 1.</param>
    /// <param name="searchFilter">An optional filter to return only contacts whose names, emails, or phone numbers contain the specified value. If null, no filtering is
    /// applied.</param>
    /// <param name="orderby">An optional field name by which to sort the results. If null, the default sort order is used.</param>
    /// <param name="order">The direction in which to sort the results. Specify ascending or descending.</param>
    /// <returns>A result containing a paged response of contact data transfer objects. If no contacts match the criteria, the response
    /// contains an empty collection.</returns>
    Task<Result<PagedResponse<ListContactDto>>> GetAllContacts(int pageSize, int pageNumber, string? searchFilter, string? orderby, SortDirectionEnum order);
    /// <summary>
    /// Retrieves the details of a contact with the specified identifier.
    /// </summary>
    /// <remarks>If the request is unauthorized, the user may be redirected to the login page. The returned
    /// result will contain error information if the contact is not found or if the request fails.</remarks>
    /// <param name="contactId">The unique identifier of the contact to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{ContactDetailsDto}"/> object with the contact details if found; otherwise, contains error information.</returns>
    Task<Result<ContactDetailsDto>> GetContactDetails(int contactId);
    /// <summary>
    /// Retrieves the jobs associated with a contact with pagination.
    /// </summary>
    /// <remarks>If the request is unauthorized, the user may be redirected to the login page. The returned
    /// result will contain error information if the request fails.</remarks>
    /// <param name="contactId">The unique identifier of the contact.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="order">The sort direction (ascending or descending).</param>
    /// <param name="orderby">Optional field name to sort by.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{PagedResponse{ListJobDto}}"/> object with the paged list of jobs if found; otherwise, contains error information.</returns>
    Task<Result<PagedResponse<ListJobDto>>> GetContactJobs(int contactId, int page, int pageSize, SortDirectionEnum order, string? orderby);
}
