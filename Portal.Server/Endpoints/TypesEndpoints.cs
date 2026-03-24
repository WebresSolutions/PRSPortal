using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Types;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Controllers;

/// <summary>
/// Static class containing types-related API endpoint definitions.
/// Provides RESTful endpoints for lookup/type data.
/// </summary>
public static class TypesEndpoints
{
    public static void AddTypesEndpoints(this WebApplication app, bool reqAuth = true)
    {
        RouteGroupBuilder appGroup = app.MapGroup("/api/types");

        appGroup.MapGet("all",
            async ([FromServices] ITypesService typesService) =>
            {
                Result<AllSettingsTypesDto> res = await typesService.GetAllSettingsTypes();
                return EndpointsHelper.ProcessResult(res, "An error occurred loading settings types");
            })
            .WithSummary("Get all settings lookup data")
            .WithDescription("Returns timesheet, contact, job, file, task, technical contact types, job colours, states, schedule colours, and service types in one response.")
            .Produces<AllSettingsTypesDto>();

        appGroup.MapGet("service",
            async ([FromServices] ITypesService typesService) =>
            {
                Result<ServiceTypeDto[]> res = await typesService.GetServiceTypes();
                return EndpointsHelper.ProcessResult(res, "An error occurred getting service types");
            })
            .WithSummary("Get service types")
            .WithDescription("Gets the service catalog used on quotes and invoices.")
            .Produces<ServiceTypeDto[]>();
        appGroup.MapPut("service",
            async ([FromServices] ITypesService typesService, [FromBody] ServiceTypeDto dto) =>
            {
                Result<ServiceTypeDto> res = await typesService.SaveServiceType(dto);
                return EndpointsHelper.ProcessResult(res, "An error occurred saving service type");
            })
            .WithSummary("Create or update service type")
            .Produces<ServiceTypeDto>();

        appGroup.MapGet("timesheet",
            async ([FromServices] ITypesService typesService) =>
            {
                Result<TimeTypeDto[]> res = await typesService.GetTimeSheetTypes();
                return EndpointsHelper.ProcessResult(res, "An error occurred getting timesheet types");
            })
            .WithSummary("Get timesheet types")
            .WithDescription("Gets a list of timesheet entry types (e.g. Billable, Admin).")
            .Produces<TimeTypeDto[]>();
        appGroup.MapPut("timesheet",
            async ([FromServices] ITypesService typesService, [FromBody] TimeTypeDto dto) =>
            {
                Result<TimeTypeDto> res = await typesService.SaveTimeSheetType(dto);
                return EndpointsHelper.ProcessResult(res, "An error occurred saving timesheet type");
            })
            .WithSummary("Create or update timesheet type")
            .Produces<TimeTypeDto>();

        appGroup.MapGet("contact",
            async ([FromServices] ITypesService typesService) =>
            {
                Result<ContactTypeDto[]> res = await typesService.GetContactTypes();
                return EndpointsHelper.ProcessResult(res, "An error occurred getting contact types");
            })
            .WithSummary("Get contact types")
            .WithDescription("Gets a list of contact types (e.g. Company, Personal).")
            .Produces<ContactTypeDto[]>();
        appGroup.MapPut("contact",
            async ([FromServices] ITypesService typesService, [FromBody] ContactTypeDto dto) =>
            {
                Result<ContactTypeDto> res = await typesService.SaveContactType(dto);
                return EndpointsHelper.ProcessResult(res, "An error occurred saving contact type");
            })
            .WithSummary("Create or update contact type")
            .Produces<ContactTypeDto>();

        appGroup.MapGet("job",
            async ([FromServices] ITypesService typesService) =>
            {
                Result<JobTypeDto[]> res = await typesService.GetJobTypes();
                return EndpointsHelper.ProcessResult(res, "An error occurred getting job types");
            })
            .WithSummary("Get job types")
            .WithDescription("Gets a list of job types (e.g. Construction, Survey).")
            .Produces<JobTypeDto[]>();
        appGroup.MapPut("job",
            async ([FromServices] ITypesService typesService, [FromBody] JobTypeDto dto) =>
            {
                Result<JobTypeDto> res = await typesService.SaveJobType(dto);
                return EndpointsHelper.ProcessResult(res, "An error occurred saving job type");
            })
            .WithSummary("Create or update job type")
            .Produces<JobTypeDto>();

        appGroup.MapGet("jobcolour",
            async ([FromServices] ITypesService typesService) =>
            {
                Result<JobColourDto[]> res = await typesService.GetJobColours();
                return EndpointsHelper.ProcessResult(res, "An error occurred getting job colours");
            })
            .WithSummary("Get job colours")
            .WithDescription("Gets a list of job colours.")
            .Produces<JobColourDto[]>();
        appGroup.MapPut("jobcolour",
            async ([FromServices] ITypesService typesService, [FromBody] JobColourDto dto) =>
            {
                Result<JobColourDto> res = await typesService.SaveJobColour(dto);
                return EndpointsHelper.ProcessResult(res, "An error occurred saving job colour");
            })
            .WithSummary("Create or update job colour")
            .Produces<JobColourDto>();

        appGroup.MapGet("schedulecolour",
            async ([FromServices] ITypesService typesService) =>
            {
                Result<ScheduleColourDto[]> res = await typesService.GetScheduleColours();
                return EndpointsHelper.ProcessResult(res, "An error occurred getting schedule colours");
            })
            .WithSummary("Get schedule colours")
            .WithDescription("Gets a list of schedule colours.")
            .Produces<ScheduleColourDto[]>();

        appGroup.MapGet("file",
            async ([FromServices] ITypesService typesService) =>
            {
                Result<FileTypeDto[]> res = await typesService.GetFileTypes();
                return EndpointsHelper.ProcessResult(res, "An error occurred getting file types");
            })
            .WithSummary("Get file types")
            .WithDescription("Gets a list of file types.")
            .Produces<FileTypeDto[]>();
        appGroup.MapPut("file",
            async ([FromServices] ITypesService typesService, [FromBody] FileTypeDto dto) =>
            {
                Result<FileTypeDto> res = await typesService.SaveFileType(dto);
                return EndpointsHelper.ProcessResult(res, "An error occurred saving file type");
            })
            .WithSummary("Create or update file type")
            .Produces<FileTypeDto>();

        appGroup.MapGet("jobtask",
            async ([FromServices] ITypesService typesService) =>
            {
                Result<JobTaskTypeDto[]> res = await typesService.GetJobTaskTypes();
                return EndpointsHelper.ProcessResult(res, "An error occurred getting job task types");
            })
            .WithSummary("Get job task types")
            .WithDescription("Gets a list of job task types.")
            .Produces<JobTaskTypeDto[]>();
        appGroup.MapPut("jobtask",
            async (HttpContext httpContext, [FromServices] ITypesService typesService, [FromBody] JobTaskTypeDto dto) =>
            {
                Result<JobTaskTypeDto> res = await typesService.SaveJobTaskType(httpContext, dto);
                return EndpointsHelper.ProcessResult(res, "An error occurred saving job task type");
            })
            .WithSummary("Create or update job task type")
            .Produces<JobTaskTypeDto>();

        appGroup.MapGet("technicalcontact",
            async ([FromServices] ITypesService typesService) =>
            {
                Result<TechnicalContactTypeDto[]> res = await typesService.GetTechnicalContactTypes();
                return EndpointsHelper.ProcessResult(res, "An error occurred getting technical contact types");
            })
            .WithSummary("Get technical contact types")
            .WithDescription("Gets a list of technical contact types.")
            .Produces<TechnicalContactTypeDto[]>();
        appGroup.MapPut("technicalcontact",
            async ([FromServices] ITypesService typesService, [FromBody] TechnicalContactTypeDto dto) =>
            {
                Result<TechnicalContactTypeDto> res = await typesService.SaveTechnicalContactType(dto);
                return EndpointsHelper.ProcessResult(res, "An error occurred saving technical contact type");
            })
            .WithSummary("Create or update technical contact type")
            .Produces<TechnicalContactTypeDto>();

        appGroup.MapGet("state",
            async ([FromServices] ITypesService typesService) =>
            {
                Result<StateDto[]> res = await typesService.GetStates();
                return EndpointsHelper.ProcessResult(res, "An error occurred getting states");
            })
            .WithSummary("Get states")
            .WithDescription("Gets a list of states/territories (e.g. NSW, VIC).")
            .Produces<StateDto[]>();

        if (reqAuth)
            appGroup.RequireAuthorization();
        else
            appGroup.AllowAnonymous();
    }
}
