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

namespace Portal.Client.Services.Interfaces;

public interface IApiService
{
    #region JOBS
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
    Task<Result<PagedResponse<ListJobDto>>> GetAllJobs(JobFilterDto filter);
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
    /// Creates a new job with the provided details.
    /// </summary>
    /// <param name="data">The job creation data.</param>
    /// <returns>A result containing the new job ID on success.</returns>
    Task<Result<int>> CreateJob(JobCreationDto data);
    /// <summary>
    /// Soft deletes a Job
    /// </summary>
    /// <param name="jobId"></param>
    /// <returns></returns>
    Task<Result<bool>> DeleteJob(int jobId);
    /// <summary>
    /// Updates a job   
    /// </summary>
    /// <param name="data">The job being updated</param>
    /// <returns></returns>
    Task<Result<JobDetailsDto>> UpdateJob(JobDetailsDto data);
    /// <summary>
    /// Gets the notes for a job.
    /// </summary>
    /// <param name="jobId">The job id.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted notes.</param>
    /// <returns>The list of notes for the job.</returns>
    Task<Result<List<JobNoteDto>>> GetJobNotes(int jobId, bool includeDeleted = false, bool? actionRequired = null);
    /// <summary>
    /// Creates or updates a job note. Use NoteId 0 to create; set NoteId to the existing note id to update.
    /// </summary>
    /// <param name="note">The note to create or update.</param>
    /// <returns>The updated job details including notes.</returns>
    Task<Result<List<JobNoteDto>>> SaveJobNote(JobNoteDto note);
    /// <summary>
    /// Gets technical contacts filtered by jobId and/or contactId. At least one of jobId or contactId must be provided.
    /// </summary>
    /// <param name="jobId">Optional job id to filter by.</param>
    /// <param name="contactId">Optional contact id to filter by.</param>
    /// <param name="showDeleted">Whether to include soft-deleted technical contacts.</param>
    /// <returns>The list of technical contacts.</returns>
    Task<Result<TechnicalContactDto[]>> GetTechnicalContacts(int? jobId, int? contactId, bool showDeleted = false);
    /// <summary>
    /// Creates or updates a technical contact (links a contact to a job with a role). Use Id 0 to create; set Id to the existing technical contact id to update.
    /// </summary>
    /// <param name="dto">The technical contact to create or update.</param>
    /// <returns>The updated list of technical contacts for the job.</returns>
    Task<Result<TechnicalContactDto[]>> SaveTechnicalContact(SaveTechnicalContactTypeDto dto);
    /// <summary>
    /// Saves (uploads or replaces) a file for a job.
    /// </summary>
    /// <param name="jobId">The job id to attach the file to.</param>
    /// <param name="file">The file DTO with content, name, type, etc.</param>
    /// <returns>The file id on success.</returns>
    Task<Result<int>> SaveJobFile(int jobId, FileDto file);
    #endregion

    #region SCHEDULE
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
    /// Retrieves the current system settings from the server asynchronously.
    /// </summary>
    /// <remarks>If the user is not authorized, the method may trigger navigation to the login page. The
    /// returned result object includes error details if the operation fails, such as when the server returns an error
    /// response.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{SystemSettingDto}"/> object with the retrieved system settings if the request is successful;
    /// otherwise, contains error information.</returns>
    Task<Result<ScheduleColourDto>> UpdateScheduleColour(ScheduleColourDto colour);
    #endregion

    #region SETTINGS
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
    /// Updates the system settings with the specified values asynchronously.
    /// </summary>
    /// <remarks>If the operation is unauthorized, the user may be redirected to the login page. The returned
    /// Result object provides details about the success or failure of the update operation.</remarks>
    /// <param name="settings">An object containing the new system settings to apply. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the updated
    /// system settings if the update is successful; otherwise, contains error information.</returns>
    Task<Result<SystemSettingDto>> UpdateSystemSettings(SystemSettingDto settings);
    #endregion

    #region COUNCILS
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
    #endregion

    #region CONTACTS
    /// <summary>
    /// Retrieves a paged list of contacts, optionally filtered by split search fields (name, email, phone, address, contactId) or searchFilter for type-ahead, and sorted.
    /// </summary>
    /// <remarks>If the request is unauthorized, the user may be redirected to the login page. The method does
    /// not throw exceptions for HTTP errors; instead, error information is included in the returned result.</remarks>
    /// <param name="filter">Filter parameters including page, pageSize, order, orderby, deleted, and optional search fields (nameSearch, emailSearch, phoneSearch, addressSearch, contactIdSearch) or searchFilter for type-ahead.</param>
    /// <returns>A result containing a paged response of contact data transfer objects. If no contacts match the criteria, the response
    /// contains an empty collection.</returns>
    Task<Result<PagedResponse<ListContactDto>>> GetAllContacts(ContactFilterDto filter);
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
    /// Creates a new contact with the provided details.
    /// </summary>
    /// <param name="data">The contact creation data.</param>
    /// <returns>A result containing the new contact ID on success.</returns>
    Task<Result<int>> CreateContact(ContactCreationDto data);
    /// <summary>
    /// Updates an existing contact with the provided details.
    /// </summary>
    /// <param name="data">The contact update data.</param>
    /// <returns>A result containing the updated contact details on success.</returns>
    Task<Result<ContactDetailsDto>> UpdateContact(ContactUpdateDto data);
    #endregion

    #region USERS
    /// <summary>
    /// Gets notes for a particular user. If the user is not authorized, the method may trigger navigation to the login page. The returned result contains error information if the request fails.
    /// </summary>
    /// <returns></returns>
    Task<Result<JobNoteDto[]>> GetUserNotes(bool includeDeleted = false, bool? actionRequired = null);
    /// <summary>
    /// Gets a list of users
    /// </summary>
    /// <returns>An array of users</returns>
    Task<Result<UserDto[]>> GetUsersList();
    #endregion

    #region TIMESHEETS 
    /// <summary>
    /// Gets timesheet entries for a user within a date range. Use userId 0 for the current user.
    /// </summary>
    Task<Result<TimeSheetDto[]>> GetUserTimeSheets(int userId, DateTime start, DateTime? end);
    /// <summary>
    /// Adds a new timesheet entry for the current user.
    /// </summary>
    Task<Result<TimeSheetDto>> AddTimeSheetEntry(TimeSheetDto entry);
    /// <summary>
    /// Updates a timesheet entry for the current user. The entry must have a valid ID corresponding to an existing timesheet entry. 
    /// If the entry does not exist or the update fails, the returned result will contain error information.
    /// </summary>
    /// <param name="entry">The timesheet entry being updated</param>
    /// <returns></returns>
    Task<Result<TimeSheetDto>> UpdateTimeSheet(TimeSheetDto entry);
    /// <summary>
    /// Deletes a timesheet entry for the current user. The entry must have a valid ID corresponding to an existing timesheet entry. 
    /// If the entry does not exist or the update fails, the returned result will contain error information.
    /// </summary>
    /// <param name="entry">The timesheet entry being updated</param>
    /// <returns></returns>
    Task<Result<bool>> DeleteTimeSheetEntry(TimeSheetDto entry);
    /// <summary>
    /// Gets the list of timesheet entry types (e.g. Billable, Admin).
    /// </summary>
    Task<Result<TimeTypeDto[]>> GetTimeSheetTypes();
    #endregion

    #region TYPES
    /// <summary>Gets the list of contact types.</summary>
    Task<Result<ContactTypeDto[]>> GetContactTypes();
    /// <summary>Gets the list of job types.</summary>
    Task<Result<JobTypeDto[]>> GetJobTypes();
    /// <summary>Gets the list of job colours.</summary>
    Task<Result<JobColourDto[]>> GetJobColours();
    /// <summary>Gets the list of file types.</summary>
    Task<Result<FileTypeDto[]>> GetFileTypes();
    /// <summary>Gets the list of job task types.</summary>
    Task<Result<JobTaskTypeDto[]>> GetJobTaskTypes();
    /// <summary>Gets the list of technical contact types.</summary>
    Task<Result<TechnicalContactTypeDto[]>> GetTechnicalContactTypes();
    /// <summary>Gets the list of states/territories.</summary>
    Task<Result<StateDto[]>> GetStates();
    /// <summary>Creates or updates a timesheet type (Id 0 = create).</summary>
    Task<Result<TimeTypeDto>> SaveTimeSheetType(TimeTypeDto dto);
    /// <summary>Creates or updates a contact type (Id 0 = create).</summary>
    Task<Result<ContactTypeDto>> SaveContactType(ContactTypeDto dto);
    /// <summary>Creates or updates a job type (Id 0 = create).</summary>
    Task<Result<JobTypeDto>> SaveJobType(JobTypeDto dto);
    /// <summary>Creates or updates a job colour (Id 0 = create).</summary>
    Task<Result<JobColourDto>> SaveJobColour(JobColourDto dto);
    /// <summary>Creates or updates a file type (Id 0 = create).</summary>
    Task<Result<FileTypeDto>> SaveFileType(FileTypeDto dto);
    /// <summary>Creates or updates a job task type (Id 0 = create).</summary>
    Task<Result<JobTaskTypeDto>> SaveJobTaskType(JobTaskTypeDto dto);
    /// <summary>Creates or updates a technical contact type (Id 0 = create).</summary>
    Task<Result<TechnicalContactTypeDto>> SaveTechnicalContactType(TechnicalContactTypeDto dto);
    #endregion
}
