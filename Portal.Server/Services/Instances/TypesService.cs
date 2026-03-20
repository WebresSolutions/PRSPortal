using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.DTO.Setting;
using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Instances;

public class TypesService(PrsDbContext _dbContext, ILogger<TypesService> _logger) : ITypesService
{
    public async Task<Result<TimeTypeDto[]>> GetTimeSheetTypes()
    {
        return await GetTypesAsync(
            _dbContext.TimesheetEntryTypes.Select(x => new TimeTypeDto(x.Id, x.Name, x.Description)),
            "timesheet types");
    }

    public async Task<Result<ContactTypeDto[]>> GetContactTypes()
    {
        return await GetTypesAsync(
            _dbContext.ContactTypes.Select(x => new ContactTypeDto(x.Id, x.Name, x.Description)),
            "contact types");
    }

    public async Task<Result<JobTypeDto[]>> GetJobTypes()
    {
        return await GetTypesAsync(
            _dbContext.JobTypes.Select(x => new JobTypeDto(x.Id, x.Name, x.Abbreviation)),
            "job types");
    }

    public async Task<Result<JobColourDto[]>> GetJobColours()
    {
        return await GetTypesAsync(
            _dbContext.JobColours.Select(x => new JobColourDto(x.Id, x.Color)),
            "job colours");
    }

    public async Task<Result<ScheduleColourDto[]>> GetScheduleColours()
    {
        return await GetTypesAsync(
            _dbContext.ScheduleColours.Select(x => new ScheduleColourDto
            {
                ScheduleColourId = x.Id,
                ColourHex = x.Color,
                Description = x.Description ?? string.Empty
            }),
            "schedule colours");
    }

    public async Task<Result<FileTypeDto[]>> GetFileTypes()
    {
        return await GetTypesAsync(
            _dbContext.FileTypes.Select(x => new FileTypeDto(x.Id, x.Name, x.Description)),
            "file types");
    }

    public async Task<Result<JobTaskTypeDto[]>> GetJobTaskTypes()
    {
        return await GetTypesAsync(
            _dbContext.JobTaskTypes
                .Where(x => x.DeletedAt == null)
                .Select(x => new JobTaskTypeDto(x.Id, x.Name, x.Description)),
            "job task types");
    }

    public async Task<Result<TechnicalContactTypeDto[]>> GetTechnicalContactTypes()
    {
        return await GetTypesAsync(
            _dbContext.TechnicalContactTypes.Select(x => new TechnicalContactTypeDto(x.Id, x.Name, x.Description)),
            "technical contact types");
    }

    public async Task<Result<StateDto[]>> GetStates()
    {
        return await GetTypesAsync(
            _dbContext.States.Select(x => new StateDto(x.Id, x.Name, x.Abbreviation)),
            "states");
    }

    public async Task<Result<ServiceTypeDto[]>> GetServiceTypes()
    {
        return await GetTypesAsync(
            _dbContext.ServiceTypes
                .OrderBy(x => x.ServiceName)
                .Select(x => new ServiceTypeDto(x.Id, x.Code, x.ServiceName, x.DefaultRate, x.UnitOfMeasure, x.IsActive, x.Description)),
            "service types");
    }

    public async Task<Result<AllSettingsTypesDto>> GetAllSettingsTypes()
    {
        Result<AllSettingsTypesDto> res = new();
        try
        {
            // Same DbContext cannot run multiple queries concurrently (see EF Core threading notes).
            TimeTypeDto[] timesheetTypes = await _dbContext.TimesheetEntryTypes
                .Select(x => new TimeTypeDto(x.Id, x.Name, x.Description)).ToArrayAsync();
            ContactTypeDto[] contactTypes = await _dbContext.ContactTypes
                .Select(x => new ContactTypeDto(x.Id, x.Name, x.Description)).ToArrayAsync();
            JobTypeDto[] jobTypes = await _dbContext.JobTypes
                .Select(x => new JobTypeDto(x.Id, x.Name, x.Abbreviation)).ToArrayAsync();
            JobColourDto[] jobColours = await _dbContext.JobColours
                .Select(x => new JobColourDto(x.Id, x.Color)).ToArrayAsync();
            FileTypeDto[] fileTypes = await _dbContext.FileTypes
                .Select(x => new FileTypeDto(x.Id, x.Name, x.Description)).ToArrayAsync();
            JobTaskTypeDto[] jobTaskTypes = await _dbContext.JobTaskTypes
                .Where(x => x.DeletedAt == null)
                .Select(x => new JobTaskTypeDto(x.Id, x.Name, x.Description)).ToArrayAsync();
            TechnicalContactTypeDto[] technicalContactTypes = await _dbContext.TechnicalContactTypes
                .Select(x => new TechnicalContactTypeDto(x.Id, x.Name, x.Description)).ToArrayAsync();
            StateDto[] states = await _dbContext.States
                .Select(x => new StateDto(x.Id, x.Name, x.Abbreviation)).ToArrayAsync();
            ScheduleColourDto[] scheduleColours = await _dbContext.ScheduleColours
                .Select(x => new ScheduleColourDto
                {
                    ScheduleColourId = x.Id,
                    ColourHex = x.Color,
                    Description = x.Description ?? string.Empty
                }).ToArrayAsync();
            ServiceTypeDto[] serviceTypes = await _dbContext.ServiceTypes
                .OrderBy(x => x.ServiceName)
                .Select(x => new ServiceTypeDto(x.Id, x.Code, x.ServiceName, x.DefaultRate, x.UnitOfMeasure, x.IsActive, x.Description))
                .ToArrayAsync();

            res.SetValue(new AllSettingsTypesDto
            {
                TimesheetTypes = timesheetTypes,
                ContactTypes = contactTypes,
                JobTypes = jobTypes,
                JobColours = jobColours,
                FileTypes = fileTypes,
                JobTaskTypes = jobTaskTypes,
                TechnicalContactTypes = technicalContactTypes,
                States = states,
                ScheduleColours = scheduleColours,
                ServiceTypes = serviceTypes
            });
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all settings types");
            return res.SetError(ErrorType.InternalError, "An internal error occurred while loading settings data");
        }
    }

    public async Task<Result<TimeTypeDto>> SaveTimeSheetType(TimeTypeDto dto)
    {
        Result<TimeTypeDto> res = new();
        try
        {
            string name = StringNormalizer.TrimAndTruncate(dto.Name, 50) ?? "";
            if (string.IsNullOrWhiteSpace(name))
                return res.SetError(ErrorType.BadRequest, "Name is required");
            string? description = StringNormalizer.TrimAndTruncate(dto.Description, 255);
            if (dto.Id == 0)
            {
                var e = new TimesheetEntryType { Name = name, Description = description };
                await _dbContext.TimesheetEntryTypes.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new TimeTypeDto(e.Id, e.Name, e.Description));
            }
            else
            {
                var e = await _dbContext.TimesheetEntryTypes.FindAsync(dto.Id);
                if (e is null) return res.SetError(ErrorType.BadRequest, "Timesheet type not found");
                e.Name = name;
                e.Description = description;
                await _dbContext.SaveChangesAsync();
                res.SetValue(new TimeTypeDto(e.Id, e.Name, e.Description));
            }
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save timesheet type");
            return res.SetError(ErrorType.InternalError, "An error occurred while saving timesheet type");
        }
    }

    public async Task<Result<ContactTypeDto>> SaveContactType(ContactTypeDto dto)
    {
        Result<ContactTypeDto> res = new();
        try
        {
            string name = StringNormalizer.TrimAndTruncate(dto.Name, 50) ?? "";
            if (string.IsNullOrWhiteSpace(name))
                return res.SetError(ErrorType.BadRequest, "Name is required");
            string? description = StringNormalizer.TrimAndTruncate(dto.Description, 255);
            if (dto.Id == 0)
            {
                var e = new ContactType { Name = name, Description = description, CreatedOn = DateTime.UtcNow };
                await _dbContext.ContactTypes.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new ContactTypeDto(e.Id, e.Name, e.Description));
            }
            else
            {
                var e = await _dbContext.ContactTypes.FindAsync(dto.Id);
                if (e is null) return res.SetError(ErrorType.BadRequest, "Contact type not found");
                e.Name = name;
                e.Description = description;
                e.ModifiedOn = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                res.SetValue(new ContactTypeDto(e.Id, e.Name, e.Description));
            }
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save contact type");
            return res.SetError(ErrorType.InternalError, "An error occurred while saving contact type");
        }
    }

    public async Task<Result<JobTypeDto>> SaveJobType(JobTypeDto dto)
    {
        Result<JobTypeDto> res = new();
        try
        {
            string name = StringNormalizer.TrimAndTruncate(dto.Name, 50) ?? "";
            string abbreviation = StringNormalizer.TrimAndTruncate(dto.Abbreviation, 15) ?? "";
            if (string.IsNullOrWhiteSpace(name))
                return res.SetError(ErrorType.BadRequest, "Name is required");
            if (string.IsNullOrWhiteSpace(abbreviation))
                return res.SetError(ErrorType.BadRequest, "Abbreviation is required");
            if (dto.Id == 0)
            {
                var e = new JobType { Name = name, Abbreviation = abbreviation, CreatedAt = DateTime.UtcNow };
                await _dbContext.JobTypes.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new JobTypeDto(e.Id, e.Name, e.Abbreviation));
            }
            else
            {
                var e = await _dbContext.JobTypes.FindAsync(dto.Id);
                if (e is null) return res.SetError(ErrorType.BadRequest, "Job type not found");
                e.Name = name;
                e.Abbreviation = abbreviation;
                await _dbContext.SaveChangesAsync();
                res.SetValue(new JobTypeDto(e.Id, e.Name, e.Abbreviation));
            }
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save job type");
            return res.SetError(ErrorType.InternalError, "An error occurred while saving job type");
        }
    }

    public async Task<Result<JobColourDto>> SaveJobColour(JobColourDto dto)
    {
        Result<JobColourDto> res = new();
        try
        {
            string color = (dto.colour ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(color))
                return res.SetError(ErrorType.BadRequest, "Colour is required");
            if (!color.StartsWith("#"))
                color = "#" + color;
            color = StringNormalizer.TrimAndTruncate(color, 20) ?? color;
            if (dto.Id == 0)
            {
                var e = new JobColour { Color = color, CreatedAt = DateTime.UtcNow };
                await _dbContext.JobColours.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new JobColourDto(e.Id, e.Color));
            }
            else
            {
                var e = await _dbContext.JobColours.FindAsync(dto.Id);
                if (e is null) return res.SetError(ErrorType.BadRequest, "Job colour not found");
                e.Color = color;
                await _dbContext.SaveChangesAsync();
                res.SetValue(new JobColourDto(e.Id, e.Color));
            }
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save job colour");
            return res.SetError(ErrorType.InternalError, "An error occurred while saving job colour");
        }
    }

    public async Task<Result<FileTypeDto>> SaveFileType(FileTypeDto dto)
    {
        Result<FileTypeDto> res = new();
        try
        {
            string name = StringNormalizer.TrimAndTruncate(dto.Name, 255) ?? "";
            if (string.IsNullOrWhiteSpace(name))
                return res.SetError(ErrorType.BadRequest, "Name is required");
            string? description = StringNormalizer.TrimAndTruncate(dto.Description, 255);
            if (dto.Id == 0)
            {
                var e = new FileType { Name = name, Description = description, CreatedAt = DateTime.UtcNow };
                await _dbContext.FileTypes.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new FileTypeDto(e.Id, e.Name, e.Description));
            }
            else
            {
                var e = await _dbContext.FileTypes.FindAsync(dto.Id);
                if (e is null) return res.SetError(ErrorType.BadRequest, "File type not found");
                e.Name = name;
                e.Description = description;
                await _dbContext.SaveChangesAsync();
                res.SetValue(new FileTypeDto(e.Id, e.Name, e.Description));
            }
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file type");
            return res.SetError(ErrorType.InternalError, "An error occurred while saving file type");
        }
    }

    public async Task<Result<JobTaskTypeDto>> SaveJobTaskType(HttpContext httpContext, JobTaskTypeDto dto)
    {
        Result<JobTaskTypeDto> res = new();
        try
        {
            string name = StringNormalizer.TrimAndTruncate(dto.Name, 255) ?? "";
            if (string.IsNullOrWhiteSpace(name))
                return res.SetError(ErrorType.BadRequest, "Name is required");
            string? description = StringNormalizer.TrimAndTruncate(dto.Description, 255);
            int userId = httpContext.UserId();
            if (dto.Id == 0)
            {
                var e = new JobTaskType { Name = name, Description = description, CreatedByUserId = userId, CreatedOn = DateTime.UtcNow };
                await _dbContext.JobTaskTypes.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new JobTaskTypeDto(e.Id, e.Name, e.Description));
            }
            else
            {
                var e = await _dbContext.JobTaskTypes.FindAsync(dto.Id);
                if (e is null) return res.SetError(ErrorType.BadRequest, "Job task type not found");
                e.Name = name;
                e.Description = description;
                e.ModifiedByUserId = userId;
                e.ModifiedOn = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                res.SetValue(new JobTaskTypeDto(e.Id, e.Name, e.Description));
            }
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save job task type");
            return res.SetError(ErrorType.InternalError, "An error occurred while saving job task type");
        }
    }

    public async Task<Result<TechnicalContactTypeDto>> SaveTechnicalContactType(TechnicalContactTypeDto dto)
    {
        Result<TechnicalContactTypeDto> res = new();
        try
        {
            string name = StringNormalizer.TrimAndTruncate(dto.Name, 50) ?? "";
            if (string.IsNullOrWhiteSpace(name))
                return res.SetError(ErrorType.BadRequest, "Name is required");
            string? description = StringNormalizer.TrimAndTruncate(dto.Description, 255);
            if (dto.Id == 0)
            {
                var e = new TechnicalContactType { Name = name, Description = description, CreatedOn = DateTime.UtcNow };
                await _dbContext.TechnicalContactTypes.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new TechnicalContactTypeDto(e.Id, e.Name, e.Description));
            }
            else
            {
                var e = await _dbContext.TechnicalContactTypes.FindAsync(dto.Id);
                if (e is null) return res.SetError(ErrorType.BadRequest, "Technical contact type not found");
                e.Name = name;
                e.Description = description;
                e.ModifiedOn = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                res.SetValue(new TechnicalContactTypeDto(e.Id, e.Name, e.Description));
            }
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save technical contact type");
            return res.SetError(ErrorType.InternalError, "An error occurred while saving technical contact type");
        }
    }

    public async Task<Result<ServiceTypeDto>> SaveServiceType(ServiceTypeDto dto)
    {
        Result<ServiceTypeDto> res = new();
        try
        {
            string serviceName = StringNormalizer.TrimAndTruncate(dto.ServiceName, 150) ?? "";
            if (string.IsNullOrWhiteSpace(serviceName))
                return res.SetError(ErrorType.BadRequest, "Service name is required");
            string? code = StringNormalizer.TrimAndTruncate(dto.Code, 20);
            if (string.IsNullOrWhiteSpace(code))
                code = null;
            string? uom = StringNormalizer.TrimAndTruncate(dto.UnitOfMeasure, 20);
            if (string.IsNullOrWhiteSpace(uom))
                uom = null;
            string? description = StringNormalizer.TrimAndTruncate(dto.Description, 2000);
            bool isActive = dto.IsActive != false;

            if (code is not null)
            {
                bool duplicate = await _dbContext.ServiceTypes.AnyAsync(x => x.Code == code && x.Id != dto.Id);
                if (duplicate)
                    return res.SetError(ErrorType.BadRequest, "Code is already in use");
            }

            if (dto.Id == 0)
            {
                var e = new ServiceType
                {
                    Code = code,
                    ServiceName = serviceName,
                    DefaultRate = dto.DefaultRate,
                    UnitOfMeasure = uom,
                    IsActive = isActive,
                    Description = description
                };
                await _dbContext.ServiceTypes.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new ServiceTypeDto(e.Id, e.Code, e.ServiceName, e.DefaultRate, e.UnitOfMeasure, e.IsActive, e.Description));
            }
            else
            {
                var e = await _dbContext.ServiceTypes.FindAsync(dto.Id);
                if (e is null) return res.SetError(ErrorType.BadRequest, "Service type not found");
                e.Code = code;
                e.ServiceName = serviceName;
                e.DefaultRate = dto.DefaultRate;
                e.UnitOfMeasure = uom;
                e.IsActive = isActive;
                e.Description = description;
                await _dbContext.SaveChangesAsync();
                res.SetValue(new ServiceTypeDto(e.Id, e.Code, e.ServiceName, e.DefaultRate, e.UnitOfMeasure, e.IsActive, e.Description));
            }
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save service type");
            return res.SetError(ErrorType.InternalError, "An error occurred while saving service type");
        }
    }

    private async Task<Result<T[]>> GetTypesAsync<T>(IQueryable<T> query, string typeName)
    {
        Result<T[]> res = new();
        try
        {
            res.SetValue(await query.ToArrayAsync());
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get {TypeName} ex: {}", typeName, ex);
            return res.SetError(ErrorType.InternalError, $"An internal error occurred while getting {typeName}");
        }
    }
}
