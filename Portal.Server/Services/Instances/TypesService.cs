using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Types;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Instances;

/// <summary>
/// Used for getting and updating types
/// </summary>
/// <param name="_dbContext">Database context</param>
/// <param name="_logger">Logger interface</param>
public class TypesService(PrsDbContext _dbContext, ILogger<TypesService> _logger) : ITypesService
{
    /// <inheritdoc/>
    public async Task<Result<TimeTypeDto[]>> GetTimeSheetTypes()
        => await GetTypesAsync(
            _dbContext.TimesheetEntryTypes.Where(x => x.IsActive).Select(x => new TimeTypeDto(x.Id, x.Name, x.Description)),
            "timesheet types");

    /// <inheritdoc/>
    public async Task<Result<ContactTypeDto[]>> GetContactTypes()
        => await GetTypesAsync(
            _dbContext.ContactTypes.Where(x => x.IsActive).Select(x => new ContactTypeDto(x.Id, x.Name, x.Description)),
            "contact types");

    /// <inheritdoc/>
    public async Task<Result<JobTypeDto[]>> GetJobTypes()
        => await GetTypesAsync(
            _dbContext.JobTypes.Select(x => new JobTypeDto(x.Id, x.Name, x.Abbreviation)),
            "job types");

    /// <inheritdoc/>
    public async Task<Result<JobColourDto[]>> GetJobColours()
        => await GetTypesAsync(
            _dbContext.JobColours.Where(x => x.IsActive).Select(x => new JobColourDto(x.Id, x.Color)),
            "job colours");

    /// <inheritdoc/>
    public async Task<Result<ScheduleColourDto[]>> GetScheduleColours()
        => await GetTypesAsync(
            _dbContext.ScheduleColours.Select(x => new ScheduleColourDto
            {
                ScheduleColourId = x.Id,
                ColourHex = x.Color,
                Description = x.Description ?? string.Empty
            }),
            "schedule colours");

    /// <inheritdoc/>
    public async Task<Result<FileTypeDto[]>> GetFileTypes()
        => await GetTypesAsync(
            _dbContext.FileTypes.Where(x => x.IsActive).Select(x => new FileTypeDto(x.Id, x.Name, x.Description)),
            "file types");

    /// <inheritdoc/>
    public async Task<Result<JobTaskTypeDto[]>> GetJobTaskTypes()
        => await GetTypesAsync(
            _dbContext.JobTaskTypes
                .Where(x => x.IsActive)
                .Select(x => new JobTaskTypeDto(x.Id, x.Name, x.Description)),
            "job task types");

    /// <inheritdoc/>
    public async Task<Result<TechnicalContactTypeDto[]>> GetTechnicalContactTypes()
        => await GetTypesAsync(
            _dbContext.TechnicalContactTypes.Where(x => x.IsActive).Select(x => new TechnicalContactTypeDto(x.Id, x.Name, x.Description)),
            "technical contact types");

    /// <inheritdoc/>
    public async Task<Result<StateDto[]>> GetStates()
        => await GetTypesAsync(
            _dbContext.States.Select(x => new StateDto(x.Id, x.Name, x.Abbreviation)),
            "states");

    /// <inheritdoc/>
    public async Task<Result<ServiceTypeDto[]>> GetServiceTypes()
        => await GetTypesAsync(
            _dbContext.ServiceTypes.Where(x => x.IsActive)
                .OrderBy(x => x.ServiceName)
                .Select(x => new ServiceTypeDto(x.Id, x.Code, x.ServiceName, x.IsActive, x.Description)),
            "service types");

    /// <inheritdoc/>
    public async Task<Result<JobTypeStatusDto[]>> GetJobStatuses()
        => await GetTypesAsync(
            _dbContext.JobStatuses
                .OrderBy(x => x.JobTypeId)
                .ThenBy(x => x.Sequence)
                .Select(x => new JobTypeStatusDto(x.Id, x.JobTypeId, x.Name, x.Sequence, x.Colour, x.IsActive)),
            "job statuses");

    /// <inheritdoc/>
    public async Task<Result<AllSettingsTypesDto>> GetAllSettingsTypes()
    {
        Result<AllSettingsTypesDto> res = new();
        try
        {
            // Same DbContext cannot run multiple queries concurrently (see EF Core threading notes).
            TimeTypeDto[] timesheetTypes = await _dbContext.TimesheetEntryTypes
                .Where(x => x.IsActive)
                .Select(x => new TimeTypeDto(x.Id, x.Name, x.Description)).ToArrayAsync();
            ContactTypeDto[] contactTypes = await _dbContext.ContactTypes
                .Where(x => x.IsActive)
                .Select(x => new ContactTypeDto(x.Id, x.Name, x.Description)).ToArrayAsync();
            JobTypeDto[] jobTypes = await _dbContext.JobTypes
                .Select(x => new JobTypeDto(x.Id, x.Name, x.Abbreviation)).ToArrayAsync();
            JobColourDto[] jobColours = await _dbContext.JobColours
                .Where(x => x.IsActive)
                .Select(x => new JobColourDto(x.Id, x.Color)).ToArrayAsync();
            FileTypeDto[] fileTypes = await _dbContext.FileTypes
                .Where(x => x.IsActive)
                .Select(x => new FileTypeDto(x.Id, x.Name, x.Description)).ToArrayAsync();
            JobTaskTypeDto[] jobTaskTypes = await _dbContext.JobTaskTypes
                .Where(x => x.IsActive)
                .Select(x => new JobTaskTypeDto(x.Id, x.Name, x.Description)).ToArrayAsync();
            TechnicalContactTypeDto[] technicalContactTypes = await _dbContext.TechnicalContactTypes
                .Where(x => x.IsActive)
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
                .Where(x => x.IsActive)
                .OrderBy(x => x.ServiceName)
                .Select(x => new ServiceTypeDto(x.Id, x.Code, x.ServiceName, x.IsActive, x.Description))
                .ToArrayAsync();
            JobTypeStatusDto[] jobStatuses = await _dbContext.JobStatuses
                .OrderBy(x => x.JobTypeId)
                .ThenBy(x => x.Sequence)
                .Select(x => new JobTypeStatusDto(x.Id, x.JobTypeId, x.Name, x.Sequence, x.Colour, x.IsActive))
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
                ServiceTypes = serviceTypes,
                JobStatuses = jobStatuses,
            });
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all settings types");
            return res.SetError(ErrorType.InternalError, "An internal error occurred while loading settings data");
        }
    }

    /// <inheritdoc/>
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
                TimesheetEntryType e = new() { Name = name, Description = description };
                await _dbContext.TimesheetEntryTypes.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new TimeTypeDto(e.Id, e.Name, e.Description));
            }
            else
            {
                TimesheetEntryType? e = await _dbContext.TimesheetEntryTypes.FindAsync(dto.Id);
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

    /// <inheritdoc/>
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
                ContactType e = new() { Name = name, Description = description, CreatedOn = DateTime.UtcNow };
                await _dbContext.ContactTypes.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new ContactTypeDto(e.Id, e.Name, e.Description));
            }
            else
            {
                ContactType? e = await _dbContext.ContactTypes.FindAsync(dto.Id);
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

    /// <inheritdoc/>
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
                JobType e = new() { Name = name, Abbreviation = abbreviation, CreatedAt = DateTime.UtcNow };
                await _dbContext.JobTypes.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new JobTypeDto(e.Id, e.Name, e.Abbreviation));
            }
            else
            {
                JobType? e = await _dbContext.JobTypes.FindAsync(dto.Id);
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

    /// <inheritdoc/>
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
                JobColour e = new() { Color = color, CreatedOn = DateTime.UtcNow };
                await _dbContext.JobColours.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new JobColourDto(e.Id, e.Color));
            }
            else
            {
                JobColour? e = await _dbContext.JobColours.FindAsync(dto.Id);
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

    /// <inheritdoc/>
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
                FileType e = new() { Name = name, Description = description, CreatedOn = DateTime.UtcNow };
                await _dbContext.FileTypes.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new FileTypeDto(e.Id, e.Name, e.Description));
            }
            else
            {
                FileType? e = await _dbContext.FileTypes.FindAsync(dto.Id);
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

    /// <inheritdoc/>
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
                JobTaskType e = new() { Name = name, Description = description, CreatedOn = DateTime.UtcNow };
                await _dbContext.JobTaskTypes.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new JobTaskTypeDto(e.Id, e.Name, e.Description));
            }
            else
            {
                JobTaskType? e = await _dbContext.JobTaskTypes.FindAsync(dto.Id);
                if (e is null) return res.SetError(ErrorType.BadRequest, "Job task type not found");
                e.Name = name;
                e.Description = description;
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

    /// <inheritdoc/>
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
                TechnicalContactType e = new() { Name = name, Description = description, CreatedOn = DateTime.UtcNow };
                await _dbContext.TechnicalContactTypes.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new TechnicalContactTypeDto(e.Id, e.Name, e.Description));
            }
            else
            {
                TechnicalContactType? e = await _dbContext.TechnicalContactTypes.FindAsync(dto.Id);
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

    /// <inheritdoc/>
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
                ServiceType e = new()
                {
                    Code = code,
                    ServiceName = serviceName,
                    IsActive = isActive,
                    Description = description
                };
                await _dbContext.ServiceTypes.AddAsync(e);
                await _dbContext.SaveChangesAsync();
                res.SetValue(new ServiceTypeDto(e.Id, e.Code, e.ServiceName, e.IsActive, e.Description));
            }
            else
            {
                ServiceType? e = await _dbContext.ServiceTypes.FindAsync(dto.Id);
                if (e is null) return res.SetError(ErrorType.BadRequest, "Service type not found");
                e.Code = code;
                e.ServiceName = serviceName;
                e.IsActive = isActive;
                e.Description = description;
                await _dbContext.SaveChangesAsync();
                res.SetValue(new ServiceTypeDto(e.Id, e.Code, e.ServiceName, e.IsActive, e.Description));
            }
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save service type");
            return res.SetError(ErrorType.InternalError, "An error occurred while saving service type");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<JobTypeStatusDto[]>> SaveJobTypeStatuses(IEnumerable<JobTypeStatusDto> dto)
    {
        Result<JobTypeStatusDto[]> res = new();
        if (!dto.Any())
            return res.SetError(ErrorType.BadRequest, "Job Statuses not provided");

        await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Check that the job type is valid for all of the items.
            int[] providedJobTypes = [.. dto.Select(x => x.JobTypeId).Distinct()];

            if (!await _dbContext.JobTypes.AllAsync(x => providedJobTypes.Contains(x.Id)))
                return res.SetError(ErrorType.BadRequest, "Invalid Job Type Provided");

            List<JobTypeStatusDto> distinctBySequenceAndJobtype = [.. dto.GroupBy(x => new { x.JobTypeId, x.Sequence }).Select(x => x.First())];
            if (distinctBySequenceAndJobtype.Count != dto.Count())
                return res.SetError(ErrorType.BadRequest, "The sequence is not unique");

            foreach (JobTypeStatusDto status in dto)
            {
                string statusName = StringNormalizer.TrimAndTruncate(status.Name, 100) ?? "";
                string? colour = StringNormalizer.TrimAndTruncate(status.Colour, 12);

                if (string.IsNullOrWhiteSpace(statusName))
                {
                    await transaction.RollbackAsync();
                    return res.SetError(ErrorType.BadRequest, "Status name is required");
                }
                if (string.IsNullOrWhiteSpace(colour))
                {
                    await transaction.RollbackAsync();
                    return res.SetError(ErrorType.BadRequest, "Colour is required");
                }

                // Deal with the ordering 
                if (status.Id == 0)
                {
                    JobStatus e = new()
                    {
                        Name = statusName,
                        Colour = colour,
                        CreatedAt = DateTime.UtcNow,
                        Sequence = status.Sequence,
                        JobTypeId = status.JobTypeId,
                        IsActive = status.IsActive
                    };
                    await _dbContext.JobStatuses.AddAsync(e);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    JobStatus? e = await _dbContext.JobStatuses.FindAsync(status.Id);

                    if (e is null)
                    {
                        await transaction.RollbackAsync();
                        return res.SetError(ErrorType.BadRequest, "Service type not found");
                    }
                    e.IsActive = status.IsActive;
                    e.Sequence = status.Sequence;
                    e.JobTypeId = status.JobTypeId;
                    e.Name = statusName;
                    e.Colour = colour;

                    await _dbContext.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();
            Result<JobTypeStatusDto[]> savedResult = await GetJobStatuses();
            return savedResult;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to save job status types");
            return res.SetError(ErrorType.InternalError, "An error occurred while saving the status type");
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
