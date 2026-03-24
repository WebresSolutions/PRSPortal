using Microsoft.AspNetCore.Http;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Types;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

/// <summary>
/// Interface for lookup/type data operations.
/// Defines methods for retrieving various type lists.
/// </summary>
public interface ITypesService
{
    /// <summary>
    /// Gets the list of timesheet entry types (e.g. Billable, Admin).
    /// </summary>
    Task<Result<TimeTypeDto[]>> GetTimeSheetTypes();

    /// <summary>
    /// Gets the list of contact types.
    /// </summary>
    Task<Result<ContactTypeDto[]>> GetContactTypes();

    /// <summary>
    /// Gets the list of job types.
    /// </summary>
    Task<Result<JobTypeDto[]>> GetJobTypes();

    /// <summary>
    /// Gets the list of job colours.
    /// </summary>
    Task<Result<JobColourDto[]>> GetJobColours();

    /// <summary>
    /// Gets the list of schedule colours.
    /// </summary>
    Task<Result<ScheduleColourDto[]>> GetScheduleColours();

    /// <summary>
    /// Gets the list of file types.
    /// </summary>
    Task<Result<FileTypeDto[]>> GetFileTypes();

    /// <summary>
    /// Gets the list of job task types.
    /// </summary>
    Task<Result<JobTaskTypeDto[]>> GetJobTaskTypes();

    /// <summary>
    /// Gets the list of technical contact types.
    /// </summary>
    Task<Result<TechnicalContactTypeDto[]>> GetTechnicalContactTypes();

    /// <summary>
    /// Gets the list of states/territories.
    /// </summary>
    Task<Result<StateDto[]>> GetStates();

    /// <summary>
    /// Gets service catalog line items (quotes / invoicing).
    /// </summary>
    Task<Result<ServiceTypeDto[]>> GetServiceTypes();

    /// <summary>
    /// All Settings-page type lists and schedule colours in one round trip.
    /// </summary>
    Task<Result<AllSettingsTypesDto>> GetAllSettingsTypes();

    /// <summary>Creates or updates a timesheet entry type. Use Id 0 for create.</summary>
    Task<Result<TimeTypeDto>> SaveTimeSheetType(TimeTypeDto dto);
    /// <summary>Creates or updates a contact type. Use Id 0 for create.</summary>
    Task<Result<ContactTypeDto>> SaveContactType(ContactTypeDto dto);
    /// <summary>Creates or updates a job type. Use Id 0 for create.</summary>
    Task<Result<JobTypeDto>> SaveJobType(JobTypeDto dto);
    /// <summary>Creates or updates a job colour. Use Id 0 for create.</summary>
    Task<Result<JobColourDto>> SaveJobColour(JobColourDto dto);
    /// <summary>Creates or updates a file type. Use Id 0 for create.</summary>
    Task<Result<FileTypeDto>> SaveFileType(FileTypeDto dto);
    /// <summary>Creates or updates a job task type. Use Id 0 for create.</summary>
    Task<Result<JobTaskTypeDto>> SaveJobTaskType(HttpContext httpContext, JobTaskTypeDto dto);
    /// <summary>Creates or updates a technical contact type. Use Id 0 for create.</summary>
    Task<Result<TechnicalContactTypeDto>> SaveTechnicalContactType(TechnicalContactTypeDto dto);

    /// <summary>Creates or updates a service type. Use Id 0 for create.</summary>
    Task<Result<ServiceTypeDto>> SaveServiceType(ServiceTypeDto dto);
}
