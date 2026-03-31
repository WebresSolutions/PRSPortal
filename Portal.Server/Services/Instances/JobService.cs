using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.Geometries;
using Nextended.Core.Extensions;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.File;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.DTO.Types;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;
using Quartz;
using System.Globalization;


namespace Portal.Server.Services.Instances;

/// <summary>
/// Service implementation for job-related business logic
/// Handles job retrieval, filtering, and data transformation
/// </summary>
public class JobService(PrsDbContext _dbContext, ILogger<JobService> _logger, IFileService _fileService, ISchedulerFactory _schedulerFactory) : IJobService
{
    /// <inheritdoc/>
    public async Task<Result<PagedResponse<ListJobDto>>> GetAllJobs(JobFilterDto filter)
    {
        Result<PagedResponse<ListJobDto>> result = new();
        try
        {
            IQueryable<Job> query = _dbContext.Jobs
                .AsNoTracking();

            query = !filter.Deleted ? query.Where(x => x.DeletedAt == null) : query.Where(x => x.DeletedAt != null);

            if (filter.ContactId.HasValue)
            {
                // Need to include the subcontacts of the contact id in the filter
                int[] subContacts = await _dbContext.Contacts
                    .AsNoTracking()
                    .Where(x => x.ParentContactId == filter.ContactId.Value)
                    .Select(x => x.Id)
                    .ToArrayAsync();
                query = query.Where(x => x.ContactId == filter.ContactId.Value || subContacts.Contains(x.ContactId));
            }

            if (filter.CouncilId.HasValue)
                query = query.Where(x => x.CouncilId == filter.CouncilId.Value);

            if (!string.IsNullOrWhiteSpace(filter.AddressSearch))
            {
                string pattern = PartialMatch(filter.AddressSearch);
                query = query.Where(job => job.Address != null &&
                                           job.Address.SearchVector.Matches(EF.Functions.ToTsQuery(pattern)));
            }

            if (!string.IsNullOrWhiteSpace(filter.ContactSearch))
            {
                string pattern = PartialMatch(filter.ContactSearch);
                query = query.Where(job => job.Contact != null &&
                                           job.Contact.SearchVector.Matches(EF.Functions.ToTsQuery(pattern)));
            }

            if (!string.IsNullOrWhiteSpace(filter.JobNumberSearch))
            {
                // % is the wildcard for SQL LIKE
                string pattern = $"{filter.JobNumberSearch.Trim()}%";
                query = query.Where(job => job.JobNumber != null && EF.Functions.ILike(job.JobNumber, pattern));
            }

            bool isDescending = filter.Order is SortDirectionEnum.Desc;
            query = filter.OrderBy switch
            {
                nameof(ListJobDto.JobId) => isDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                nameof(ListJobDto.Contact1.fullName) => isDescending ? query.OrderByDescending(x => x.Contact.FullName) : query.OrderBy(x => x.Contact.FullName),
                nameof(ListJobDto.JobNumber) => isDescending ? query.OrderByDescending(x => x.JobNumber) : query.OrderBy(x => x.JobNumber),
                $"{nameof(ListJobDto.Address)}.{nameof(ListJobDto.Address.Suburb)}" => isDescending ? query.OrderByDescending(x => x.Address!.Suburb) : query.OrderBy(x => x.Address!.Suburb),
                $"{nameof(ListJobDto.Address)}.{nameof(ListJobDto.Address.Street)}" => isDescending ? query.OrderByDescending(x => x.Address!.Street) : query.OrderBy(x => x.Address!.Street),
                $"{nameof(ListJobDto.Address)}.{nameof(ListJobDto.Address.PostCode)}" => isDescending ? query.OrderByDescending(x => x.Address!.PostCode) : query.OrderBy(x => x.Address!.PostCode),
                _ => query.OrderByDescending(x => x.Id)
            };

            int total = await query.CountAsync();
            int skipValue = (filter.Page - 1) * filter.PageSize;

            IQueryable<ListJobDto> jobs = query
                .Skip(skipValue)
                .Take(filter.PageSize)
                .Select(x => new ListJobDto(
                    x.Id,
                    new AddressDto(x.AddressId ?? 1, (StateEnum)x.Address!.StateId!, x.Address.StateId ?? 3, x.Address.Suburb, x.Address.Street, x.Address.PostCode),
                    x.Contact != null ? new ContactDto(x.ContactId, x.Contact.FullName) : null,
                    x.Contact != null && x.Contact.ParentContact != null ? new ContactDto(x.Contact.ParentContactId ?? 0, x.Contact.ParentContact!.FullName) : null,
                    x.JobNumber,
                    x.JobTypes.Select(x => (JobTypeEnum)x.Id).ToArray(),
                    x.Status != null ? new JobTypeStatusDto(x.Status.Id, x.Status.JobTypeId, x.Status.Name, x.Status.Sequence, x.Status.Colour, x.Status.IsActive) : null
                ));
            // Materialize the query
            List<ListJobDto> jobsList = await jobs.ToListAsync();

            result.Value = new PagedResponse<ListJobDto>(jobsList, filter.PageSize, filter.Page, total);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all jobs");
            return result.SetError(ErrorType.InternalError, "Failed to get list of jobs");
        }

        static string PartialMatch(string filter) => string.Join(" & ", filter.Split(' ', StringSplitOptions.RemoveEmptyEntries)) + ":*";
    }

    /// <inheritdoc/>
    public async Task<Result<JobDetailsDto>> GetJob(int jobId)
    {
        Result<JobDetailsDto> result = new();

        // Retrieve the job from the database
        JobDetailsDto? job = await _dbContext.Jobs
            .AsNoTracking()
            .AsSplitQuery()
            .Where(j => j.Id == jobId && j.DeletedAt == null)
            .Select(x => new JobDetailsDto
            {
                JobId = x.Id,
                JobStatusId = x.StatusId,
                JobStatusName = x.Status != null ? x.Status.Name : null,
                JobNumber = x.JobNumber,
                JobTypes = x.JobTypes.Select(x => (JobTypeEnum)x.Id).ToArray(),
                Address = x.Address != null ? new AddressDto(
                    x.AddressId ?? 1,
                    (StateEnum)x.Address!.StateId!,
                    x.Address.StateId ?? 3,
                    x.Address.Suburb,
                    x.Address.Street,
                    x.Address.PostCode,
                    x.Address.Geom != null
                        ? new LatLngDto(x.Address.Geom.X, x.Address.Geom.Y)
                        : null
                ) : null,
                Colour = x.JobColour != null
                    ? new JobColourDto(x.JobColourId!.Value, x.JobColour.Color)
                    : null,
                JobColourId = x.JobColourId,
                Description = x.Details,
                PrimaryContact = x.Contact != null
                    ? new JobContactDto(
                        x.ContactId,
                        x.Contact.FullName,
                        x.Contact.Email,
                        x.Contact.Phone ?? ""
                    )
                    : null,
                TimeSheets = x.TimesheetEntries
                    .Select(ts
                        => new TimeSheetDto(ts.Id, ts.TypeId, ts.DateFrom, ts.DateTo, ts.UserId, ts.JobId, ts.Description, ts.User.DisplayName, x.JobNumber))
                    .ToArray(),
                ContactId = x.ContactId,
                Council = x.Council != null ? new JobCouncilDto(x.Council.Id, x.Council.Name) : null,
                CouncilId = x.CouncilId,
                LastModified = x.ModifiedByUserId != null && x.ModifiedOn != null ? new LastModifiedDto(x.ModifiedOn.Value, x.ModifiedByUser!.DisplayName) : null,
                LastModifiedBy = x.ModifiedByUser != null ? x.ModifiedByUser.DisplayName : null,
                NoteCount = x.JobNotes.Count(x => x.DeletedAt == null),
                ContactCount = x.TechnicalContacts.Count(),
                DateCreated = x.CreatedOn,
                DateModified = x.ModifiedOn,
                CreatedBy = x.CreatedByUser.DisplayName,
                TargetDeliveryDate = x.TargetDeliveryDate,
                LatestClientUpdate = x.LatestClientUpdate,
                JobFiles = x.JobFiles.Select(jf => new FileDto
                {
                    JobId = x.Id,
                    FileId = jf.FileId,
                    FileName = jf.File.FileName,
                    CreatedBy = jf.CreatedByUser.DisplayName ?? "",
                    DateCreated = jf.CreatedOn,
                    DateModified = jf.File.ModifiedOn,
                    FileType = jf.File.FileType.Name,
                    FileTypeId = jf.File.FileTypeId,
                    Description = jf.File.Description,
                    Title = jf.File.Title ?? ""
                }).ToArray(),
                AssignedUsers = x.JobUsers
                    .Where(ju => ju.DeletedAt == null)
                    .Select(ju => new UserAssignmentDto(ju.User.DisplayName, ju.UserId, jobId, (JobAssignementTypeEnum)ju.AssignmentTypeId))
                    .ToArray(),
                JobHistoryDtos = x.JobStatusHistories
                    .OrderByDescending(x => x.DateChanged)
                    .Select(jh => new JobHistoryDto(
                        jobId,
                        new JobTypeStatusDto(jh.StatusIdNew, jh.JobId, jh.StatusIdNewNavigation.Name, 0, jh.StatusIdNewNavigation.Colour, true),
                        jh.DateChanged,
                        jh.ModifiedByUser.DisplayName))
                    .ToArray()
            })
            .FirstOrDefaultAsync();

        if (job is null)
            return result.SetError(ErrorType.NotFound, "Invalid job Id");

        // Get the job site visits from the schedules table
        IQueryable<JobSiteVisitsDto> query = _dbContext.Schedules
            .AsNoTracking()
            .AsSplitQuery()
            .OrderByDescending(x => x.StartTime)
            .Where(s => s.JobId == jobId)
            .Select(s => new JobSiteVisitsDto
            {
                ScheduleId = s.Id,
                TrackDate = s.ScheduleTrack.Date!.Value,
                Assignees = s.ScheduleTrack.ScheduleUsers
                    .Select(su => su.User.DisplayName)
                    .ToArray(),
                Start = s.StartTime,
                End = s.EndTime,
                Category = s.ScheduleTrack.JobType.Name,
                Notes = s.Notes ?? string.Empty
            });
        job.SiteVisits = await query.ToArrayAsync();

        job.Tasks = await _dbContext.JobTasks
            .AsNoTracking()
            .Where(t => t.JobId == jobId && t.DeletedAt == null)
            .Select(t => new JobTaskDto
            {
                Id = t.Id,
                JobId = t.JobId,
                Description = t.Description,
                InvoiceRequired = t.InvoiceRequired,
                ActiveDate = t.ActiveDate,
                CompletedDate = t.CompletedDate,
                InvoicedDate = t.InvoicedDate,
                CreatedOn = t.CreatedOn,
                CreatedByUser = t.CreatedByUser.DisplayName ?? "",
                QuotedPrice = t.QuotedPrice
            })
            .ToArrayAsync();

        job.SiteVisitCount = job.SiteVisits.Length;
        job.TaskCount = job.Tasks.Length;
        job.JobTypeStatuses = await _dbContext.JobStatuses
            .Where(x => job.JobTypes.Select(x => (int)x).Contains(x.JobTypeId) && x.IsActive)
            .Select(x => new JobTypeStatusDto(x.Id, x.JobTypeId, x.Name, x.Sequence, x.Colour, x.IsActive))
            .ToArrayAsync();

        return result.SetValue(job);
    }

    /// <inheritdoc/>
    public async Task<Result<int>> CreateJob(HttpContext httpContext, JobCreationDto data)
    {
        await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
        Result<int> result = new();
        try
        {
            // Validation
            if (await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == data.ContactId) is null)
                return result.SetError(ErrorType.BadRequest, "Invalid contact id supplied.");

            if (data.CouncilId is not null && await _dbContext.Councils.FirstOrDefaultAsync(x => x.Id == data.CouncilId) is null)
                return result.SetError(ErrorType.BadRequest, "Invalid council id supplied.");

            if (data.JobColourId is not null && await _dbContext.JobColours.FirstOrDefaultAsync(x => x.Id == data.JobColourId) is null)
                return result.SetError(ErrorType.BadRequest, "Invalid job colour id supplied.");

            if (!await _dbContext.AppUsers.AnyAsync(a => a.Id == data.ResponsibleTeamMember))
                return result.SetError(ErrorType.BadRequest, "Invalid user Id provided");

            if (!await JobAssignmentLookupIsCompleteAsync())
                return result.SetError(ErrorType.BadRequest, "Job assignment types are missing. Seed job_assignment_type with ids 1 (Current Owner) and 2 (Responsible Team Member).");

            DateTime now = DateTime.UtcNow;
            DateTime? targetDeliveryDate = data.TargetDeliveryDate is not null ? DateTime.SpecifyKind(data.TargetDeliveryDate!.Value.ToUniversalTime(), DateTimeKind.Utc) : null;
            DateTime? latestClientUpdate = data.LatestClientUpdate is not null ? DateTime.SpecifyKind(data.LatestClientUpdate!.Value.ToUniversalTime(), DateTimeKind.Utc) : null;

            if (targetDeliveryDate is not null && targetDeliveryDate < now)
                return result.SetError(ErrorType.BadRequest, "Target delivery date cannot be in the past.");

            JobType? construction = await _dbContext.JobTypes.FindAsync(1);
            JobType? survey = await _dbContext.JobTypes.FindAsync(2);

            if (construction is null || survey is null)
                throw new InvalidOperationException("job_type must contain id 1 (Construction) or 2 (Survey) before job_to_type.");

            if (await _dbContext.JobStatuses.FindAsync(data.StatusId) is not JobStatus status)
                return result.SetError(ErrorType.BadRequest, "Invalid job status.");


            string jobNumber = await CreateJobNumber();
            Job job = new()
            {
                ContactId = data.ContactId,
                CouncilId = data.CouncilId,
                JobColourId = data.JobColourId,
                Details = data.Details,
                CreatedByUserId = httpContext.UserId(),
                CreatedOn = now,
                JobNumber = jobNumber,
                StatusId = data.StatusId,
                Status = status,
                TargetDeliveryDate = targetDeliveryDate,
                LatestClientUpdate = latestClientUpdate
            };

            if (data.Address is not null)
            {
                if (data.Address.AddressId is not 0)
                {
                    if (await _dbContext.Addresses.FindAsync(data.Address.AddressId) is not Address address)
                        return result.SetError(ErrorType.BadRequest, $"Could not find the address {data.Address.AddressId} associated with the new Job.");

                    job.Address = address;
                    job.AddressId = address.Id;
                }
                else
                {

                    Address address = new()
                    {
                        Street = data.Address.Street,
                        PostCode = data.Address.PostCode,
                        Suburb = data.Address.Suburb,
                        StateId = (int?)data.Address.State ?? (int)StateEnum.VIC,
                        CreatedByUserId = httpContext.UserId(),
                        Country = "AUS",
                        CreatedOn = now
                    };

                    if (data.Address.LatLng is not null)
                    {
                        Point latlng = new(new Coordinate(data.Address.LatLng.Latitude, data.Address.LatLng.Longitude));
                        address.Geom = latlng;
                    }
                    await _dbContext.Addresses.AddAsync(address);
                    await _dbContext.SaveChangesAsync();

                    job.Address = address;
                }
            }
            // Create the other objects first

            await _dbContext.Jobs.AddAsync(job);
            await _dbContext.SaveChangesAsync();

            // Add the job types
            foreach (JobTypeEnum typeEnum in data.JobType.Distinct())
            {
                JobType type = typeEnum is JobTypeEnum.Construction ? construction : survey;
                job.JobTypes.Add(type);
            }

            // Add the Job Users
            if (data.ResponsibleTeamMember is not null)
            {
                JobUser[] jobUsers = [
                    new() { AssignmentTypeId = (int)JobAssignementTypeEnum.Responsible,
                    UserId = data.ResponsibleTeamMember.Value,
                    JobId = job.Id,
                    Job = job,
                    CreatedByUserId = httpContext.UserId(),
                    CreatedOn = now
                    }];
                await _dbContext.JobUsers.AddRangeAsync(jobUsers);
            }

            await _dbContext.SaveChangesAsync();

            // Create the sharepoint data structure in a background task
            await FileHelper.CreateJobSharepointStructure(_schedulerFactory, job.Id);
            // Commit the values to the database
            await transaction.CommitAsync();
            return result.SetValue(job.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to create job");
            return result.SetError(ErrorType.InternalError, "Failed to create job");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<JobDetailsDto>> UpdateJob(HttpContext httpContext, JobUpdateDto data)
    {
        Result<JobDetailsDto> result = new();
        try
        {
            Job? job = await _dbContext.Jobs
                .AsSplitQuery()
                .Include(j => j.Address)
                .Include(j => j.JobTypes)
                .Include(j => j.JobUsers)
                .Where(j => j.Id == data.JobId && j.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (job is null)
                return result.SetError(ErrorType.NotFound, "Invalid job Id");

            if (data.JobTypes.Length < 1)
                return result.SetError(ErrorType.BadRequest, "The job must contain at least one job type");

            if (data.ResponsibleTeamMember is not null && !await _dbContext.AppUsers.AnyAsync(a => a.Id == data.ResponsibleTeamMember))
                return result.SetError(ErrorType.BadRequest, "Invalid user Id provided");

            if (!await JobAssignmentLookupIsCompleteAsync())
                return result.SetError(ErrorType.BadRequest, "Job assignment types are missing. Seed job_assignment_type with ids 1 (Current Owner) and 2 (Responsible Team Member).");

            DateTime now = DateTime.UtcNow;

            DateTime? targetDeliveryDate = data.TargetDeliveryDate is not null ? DateTime.SpecifyKind(data.TargetDeliveryDate!.Value.ToUniversalTime(), DateTimeKind.Utc) : null;
            DateTime? latestClientUpdate = data.LatestClientUpdate is not null ? DateTime.SpecifyKind(data.LatestClientUpdate!.Value.ToUniversalTime(), DateTimeKind.Utc) : null;

            if (targetDeliveryDate is not null && targetDeliveryDate < now)
                return result.SetError(ErrorType.BadRequest, "Target delivery date cannot be in the past.");

            job.LatestClientUpdate = latestClientUpdate;
            job.TargetDeliveryDate = targetDeliveryDate;
            job.JobColourId = data.JobColourId;
            job.ModifiedByUserId = httpContext.UserId();
            job.ModifiedOn = now;

            if (data.Details is not null && data.Details.Length > 4000)
                job.Details = data.Details[..4000].Trim();
            else
                job.Details = data.Details;

            job.JobTypes.Clear();
            int[] desiredTypeIds = [.. data.JobTypes.Select(x => (int)x).Distinct()];
            foreach (int typeId in desiredTypeIds)
            {
                JobType? jobType = await _dbContext.JobTypes.FindAsync(typeId);
                if (jobType is null)
                    return result.SetError(ErrorType.BadRequest, $"Invalid job type id: {typeId}");

                job.JobTypes.Add(jobType);
            }

            if (data.ContactId != job.ContactId)
            {
                Contact? contact = await _dbContext.Contacts.FirstOrDefaultAsync(c => c.Id == data.ContactId);
                if (contact is null)
                    return result.SetError(ErrorType.BadRequest, "Invalid Contact Id");

                job.ContactId = data.ContactId;
                job.Contact = contact;
            }

            if (data.CouncilId != job.CouncilId)
            {
                if (data.CouncilId is null)
                {
                    job.Council = null;
                    job.CouncilId = null;
                }
                else
                {
                    Council? council = await _dbContext.Councils.FirstOrDefaultAsync(c => c.Id == data.CouncilId);
                    if (council is null)
                        return result.SetError(ErrorType.BadRequest, "Invalid Contact Id");

                    job.CouncilId = data.CouncilId;
                    job.Council = council;
                }
            }

            if (job.Address is not null && data.Address is not null)
            {
                job.Address.Street = data.Address.Street;
                job.Address.PostCode = data.Address.PostCode;
                job.Address.Suburb = data.Address.Suburb;
                job.Address.StateId = data.Address.State is null ? (int)StateEnum.VIC : (int)data.Address.State;
                job.Address.ModifiedByUserId = httpContext.UserId();
                job.Address.ModifiedOn = now;

                if (data.Address.LatLng is null)
                    job.Address.Geom = null;
                else if (data.Address.LatLng is LatLngDto latLng)
                    job.Address.Geom = new(new Coordinate(latLng.Latitude, latLng.Longitude));
            }
            else
            {
                if (data.Address is not null)
                {
                    job.Address = new Address
                    {
                        Street = data.Address.Street,
                        PostCode = data.Address.PostCode,
                        Suburb = data.Address.Suburb,
                        StateId = data.Address.State is null ? (int)StateEnum.VIC : (int)data.Address.State,
                        CreatedByUserId = httpContext.UserId(),
                        Country = "AUS"
                    };

                    if (data.Address.LatLng is not null)
                    {
                        Point latlng = new(new Coordinate(data.Address.LatLng.Latitude, data.Address.LatLng.Longitude));
                        job.Address.Geom = latlng;
                    }

                    await _dbContext.AddAsync(job.Address);
                    await _dbContext.SaveChangesAsync();
                    job.AddressId = job.Address.Id;
                }
            }

            if (data.JobStatusId != job.StatusId)
            {
                if (data.JobStatusId is null)
                {
                    job.StatusId = null;
                }
                else
                {
                    int newId = data.JobStatusId.Value;
                    int[] jobTypeIds = [.. job.JobTypes.Select(t => t.Id)];
                    bool statusAllowed = await _dbContext.JobStatuses
                        .AsNoTracking()
                        .AnyAsync(s => s.Id == newId && jobTypeIds.Contains(s.JobTypeId));

                    if (!statusAllowed)
                        return result.SetError(ErrorType.BadRequest, "Invalid job status for this job's type(s).");

                    int? previousStatusId = job.StatusId;
                    job.StatusId = newId;

                    await _dbContext.JobStatusHistories.AddAsync(new JobStatusHistory
                    {
                        JobId = job.Id,
                        StatusIdOld = previousStatusId is null ? newId : previousStatusId.Value,
                        StatusIdNew = newId,
                        DateChanged = now,
                        ModifiedByUserId = httpContext.UserId()
                    });
                }
            }

            JobUser? existingResponsible = job.JobUsers.FirstOrDefault(x => x.AssignmentTypeId == (int)JobAssignementTypeEnum.Responsible);
            if (job.JobUsers.Count != 1 || existingResponsible?.UserId != data.ResponsibleTeamMember)
            {
                job.JobUsers.Clear();
                if (data.ResponsibleTeamMember is not null)
                    job.JobUsers.Add(new JobUser
                    {
                        AssignmentTypeId = (int)JobAssignementTypeEnum.Responsible,
                        UserId = data.ResponsibleTeamMember.Value,
                        CreatedByUserId = httpContext.UserId(),
                        CreatedOn = now
                    });
            }
            await _dbContext.SaveChangesAsync();
            // Return the updated job details
            return await GetJob(data.JobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update job with ID {JobId}", data.JobId);
            return result.SetError(ErrorType.InternalError, "Failed to update job");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> DeleteJob(HttpContext httpContext, int id)
    {
        Result<bool> result = new();
        try
        {
            if (await _dbContext.Jobs.FirstOrDefaultAsync(x => x.Id == id) is Job job)
            {
                job.DeletedAt = DateTime.UtcNow;
                job.ModifiedByUserId = httpContext.UserId();
                await _dbContext.SaveChangesAsync();
                return result.SetValue(true);
            }

            return result.SetError(ErrorType.BadRequest, "Could not find Job with matching Id");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete job with ID {JobId}", id);
            return result.SetError(ErrorType.InternalError, "Failed to delete job");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<List<JobNoteDto>>> CreateNote(HttpContext httpContext, JobNoteDto note)
    {
        Result<List<JobNoteDto>> result = new();
        try
        {
            // Validate the object
            if (string.IsNullOrEmpty(note.Content))
                return result.SetError(ErrorType.BadRequest, "Invalid job Id supplied");

            if (await _dbContext.Jobs.FirstOrDefaultAsync(x => x.Id == note.JobId) is not Job job)
                return result.SetError(ErrorType.BadRequest, "Invalid job Id supplied");

            if (note.AssignedUser is not null && await _dbContext.AppUsers.FirstOrDefaultAsync(x => x.Id == note.AssignedUser.userId) is not AppUser user)
                return result.SetError(ErrorType.BadRequest, "Invalid assigned user Id supplied");

            JobNote jobNote = new()
            {
                JobId = note.JobId,
                Job = job,
                AssignedUserId = note.AssignedUser?.userId,
                Note = note.Content,
                CreatedByUserId = httpContext.UserId(),
                CreatedOn = DateTime.UtcNow,
                ActionRequired = note.ActionRequired
            };
            await _dbContext.JobNotes.AddAsync(jobNote);
            await _dbContext.SaveChangesAsync();

            Result<List<JobNoteDto>> notes = await GetJobNotes(note.JobId, false, null);
            return result.SetValue(notes.Value ?? throw new Exception("Failed to get the job notes"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create new note");
            return result.SetError(ErrorType.InternalError, "Failed to create note");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<List<JobNoteDto>>> UpdateNote(HttpContext httpContext, JobNoteDto note)
    {
        Result<List<JobNoteDto>> result = new();
        try
        {
            if (note.NoteId <= 0)
                return result.SetError(ErrorType.BadRequest, "Invalid note Id supplied");

            JobNote? jobNote = await _dbContext.JobNotes.FirstOrDefaultAsync(x => x.Id == note.NoteId);
            if (jobNote is null)
                return result.SetError(ErrorType.NotFound, "Note not found");

            if (note.AssignedUser is not null && await _dbContext.AppUsers.FirstOrDefaultAsync(x => x.Id == note.AssignedUser.userId) is not AppUser user)
                return result.SetError(ErrorType.BadRequest, "Invalid assigned user Id supplied");

            jobNote.Note = note.Content ?? jobNote.Note;
            jobNote.ActionRequired = note.ActionRequired;
            jobNote.AssignedUserId = note.AssignedUser?.userId;
            jobNote.ModifiedByUserId = httpContext.UserId();
            jobNote.ModifiedOn = DateTime.UtcNow;
            jobNote.DeletedAt = note.Deleted ? DateTime.UtcNow : null;

            await _dbContext.SaveChangesAsync();

            Result<List<JobNoteDto>> notes = await GetJobNotes(note.JobId, false, null);
            return result.SetValue(notes.Value ?? throw new Exception("Failed to get the job after update"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update note {NoteId}", note.NoteId);
            return result.SetError(ErrorType.InternalError, "Failed to update note");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<List<JobNoteDto>>> GetJobNotes(int jobId, bool deleted = false, bool? actionRequired = null)
    {
        Result<List<JobNoteDto>> result = new();
        try
        {
            if (jobId <= 0)
                return result.SetError(ErrorType.BadRequest, "Invalid job Id");

            if (await _dbContext.Jobs.AsNoTracking().AnyAsync(x => x.Id == jobId) is false)
                return result.SetError(ErrorType.NotFound, "Job not found");

            IQueryable<JobNote> query = _dbContext.JobNotes
                .AsNoTracking()
                .Where(n => n.JobId == jobId);

            query = deleted ? query.Where(n => n.DeletedAt != null) : query.Where(n => n.DeletedAt == null);

            if (actionRequired.HasValue)
                query = query.Where(n => n.ActionRequired == actionRequired.Value);

            result.Value = await query
                .Select(n => new JobNoteDto
                {
                    NoteId = n.Id,
                    JobId = n.JobId,
                    Content = n.Note,
                    AssignedUser = n.AssignedUserId != null
                        ? new(n.AssignedUserId.Value, n.AssignedUser!.DisplayName ?? "")
                        : null,
                    DateCreated = n.CreatedOn,
                    ActionRequired = n.ActionRequired,
                    Deleted = n.DeletedAt != null,
                    DateModified = n.ModifiedOn,
                    CreatedBy = n.CreatedByUser.DisplayName,
                    Modifiedby = n.ModifiedByUser != null ? n.ModifiedByUser.DisplayName : null
                })
                .OrderByDescending(x => x.DateCreated)
                .ToListAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get job notes for job {JobId}", jobId);
            return result.SetError(ErrorType.InternalError, "Failed to get job notes");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<TechnicalContactDto[]>> GetTechnicalContacts(int? jobId, int? contactId, bool showDeleted = false)
    {
        Result<TechnicalContactDto[]> result = new();
        try
        {
            if (jobId is null && contactId is null)
                return result.SetError(ErrorType.BadRequest, "JobId and ContactId can not both be null");

            IQueryable<TechnicalContact> query = _dbContext.TechnicalContacts
                .AsNoTracking()
                .Where(x => (jobId == null || x.JobId == jobId) && (contactId == null || x.ContactId == contactId));

            query = showDeleted ? query.Where(x => x.DeletedAt != null) : query.Where(x => x.DeletedAt == null);

            TechnicalContactDto[] res = await query
               .Select(x =>
                   new TechnicalContactDto(
                       x.Id, x.ContactId, x.JobId, x.Job.JobNumber!, x.TypeId, x.Type.Name, x.Contact.FullName, x.Contact.Email,
                       x.Contact.Phone, x.DeletedAt != null)
                   ).ToArrayAsync();

            return result.SetValue(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get the technical contact");
            return result.SetError(ErrorType.InternalError, "Failed to get job notes");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<TechnicalContactDto[]>> NewTechnicalContact(HttpContext httpContext, SaveTechnicalContactTypeDto dto)
    {
        Result<TechnicalContactDto[]> result = new();
        try
        {
            if (await _dbContext.Contacts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.ContactId) is null)
                return result.SetError(ErrorType.BadRequest, $"Invalid Contact Id: {dto.ContactId}");

            if (await _dbContext.Jobs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.JobId) is not Job job)
                return result.SetError(ErrorType.BadRequest, $"Invalid Job Id: {dto.JobId}");

            if (await _dbContext.TechnicalContactTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.ContactTypeId) is not TechnicalContactType type)
                return result.SetError(ErrorType.BadRequest, $"Invalid Job Id: {dto.JobId}");

            await _dbContext.TechnicalContacts.AddAsync(new TechnicalContact
            {
                ContactId = dto.ContactId,
                JobId = dto.JobId,
                TypeId = dto.ContactTypeId,
                CreatedOn = DateTime.UtcNow,
                CreatedByUserId = httpContext.UserId(),
                DeletedAt = null
            });

            await _dbContext.SaveChangesAsync();

            return result.SetValue([]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create new technical contact");
            return result.SetError(ErrorType.InternalError, "Failed to get job notes");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<TechnicalContactDto[]>> UpdateTechnicalContact(HttpContext httpContext, SaveTechnicalContactTypeDto dto)
    {
        Result<TechnicalContactDto[]> result = new();
        try
        {
            if (await _dbContext.TechnicalContacts.FirstOrDefaultAsync(x => x.Id == dto.Id) is not TechnicalContact techContact)
                return result.SetError(ErrorType.BadRequest, $"Invalid Technical Contact Id: {dto.ContactId}");

            if (await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == dto.ContactId) is not Contact contact)
                return result.SetError(ErrorType.BadRequest, $"Invalid Contact Id: {dto.ContactId}");

            if (await _dbContext.Jobs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.JobId) is not Job job)
                return result.SetError(ErrorType.BadRequest, $"Invalid Job Id: {dto.JobId}");

            if (await _dbContext.TechnicalContactTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.ContactTypeId) is not TechnicalContactType type)
                return result.SetError(ErrorType.BadRequest, $"Invalid Job Id: {dto.JobId}");

            techContact.JobId = dto.JobId;
            techContact.ContactId = dto.ContactId;
            techContact.TypeId = dto.ContactTypeId;
            techContact.ModifiedOn = DateTime.UtcNow;
            techContact.ModifiedByUserId = httpContext.UserId();
            techContact.DeletedAt = dto.Deleted ? DateTime.UtcNow : null;

            await _dbContext.SaveChangesAsync();

            return result.SetValue([]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to updated new technical contact");
            return result.SetError(ErrorType.InternalError, "Failed to get job notes");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<int>> SaveJobFile(HttpContext context, int jobId, FileDto file)
    {
        Result<int> res = new();
        try
        {
            if (jobId < 0 || await _dbContext.Jobs.FindAsync(jobId) is not Job job)
                return res.SetError(ErrorType.BadRequest, "Coiuld not find job with matching Id to save.");

            // Save the file (creates new or updates existing AppFile)
            Result<AppFile> savedfile = await _fileService.SaveFile(file, context.UserId());
            if (!savedfile.IsSuccess || savedfile.Value is null)
                return res.SetError(ErrorType.BadRequest, savedfile.ErrorDescription);

            // Only create a new job-file link when adding a new file; existing file is already linked
            if (file.FileId is 0)
            {
                JobFile jobFile = new()
                {
                    CreatedByUserId = context.UserId(),
                    CreatedOn = DateTime.UtcNow,
                    File = savedfile.Value,
                    FileId = savedfile.Value.Id,
                    Job = job,
                    JobId = jobId
                };
                await _dbContext.JobFiles.AddAsync(jobFile);
            }

            await _dbContext.SaveChangesAsync();
            return res.SetValue(savedfile.Value.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save job file");
            return res.SetError(ErrorType.InternalError, "Failed to get job notes");
        }
    }

    #region Private Methods
    /// <summary>
    /// Allocates the next job number for the current calendar year (UTC), format <c>{yyyy}{sequence}</c>
    /// (e.g. <c>2025001</c>, <c>2025123</c>). Only non-deleted jobs whose number starts with that year
    /// and has a numeric suffix are considered when computing the next sequence. Legacy numbers without
    /// the year prefix do not affect numbering for the new year.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when a unique number could not be reserved after several attempts.</exception>
    internal async Task<string> CreateJobNumber()
    {
        const int maxAttempts = 20;
        string year = DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int nextSequence = await ComputeNextJobSequenceForYearAsync(year);
            string candidate = string.Concat(year, nextSequence.ToString(CultureInfo.InvariantCulture));

            bool taken = await _dbContext.Jobs
                .AnyAsync(j => j.DeletedAt == null && j.JobNumber == candidate);

            if (!taken)
                return candidate;

            _logger.LogWarning(
                "Job number {Candidate} already exists; recomputing sequence (attempt {Attempt} of {Max}).",
                candidate,
                attempt + 1,
                maxAttempts);
        }

        throw new InvalidOperationException(
            $"Could not allocate a unique job number for year {year} after {maxAttempts} attempts.");
    }

    /// <summary>
    /// Returns max numeric suffix among active jobs with numbers starting with <paramref name="yearPrefix"/> plus one.
    /// </summary>
    private async Task<int> ComputeNextJobSequenceForYearAsync(string yearPrefix)
    {
        if (yearPrefix.Length != 4 || !yearPrefix.All(char.IsAsciiDigit))
            throw new ArgumentException("Year prefix must be exactly four ASCII digits.", nameof(yearPrefix));

        List<string> jobNumbers = await _dbContext.Jobs
            .AsNoTracking()
            .Where(j => j.JobNumber != null && j.JobNumber.StartsWith(yearPrefix))
            .Select(j => j.JobNumber!)
            .ToListAsync();

        int maxSuffix = 0;
        foreach (string jobNumber in jobNumbers)
        {
            if (jobNumber.Length <= yearPrefix.Length)
                continue;

            ReadOnlySpan<char> suffix = jobNumber.AsSpan(yearPrefix.Length);
            if (suffix.IsEmpty || !IsAllAsciiDigits(suffix))
            {
                _logger.LogDebug("Skipping job number {JobNumber}: missing or non-numeric suffix after year prefix.", jobNumber);
                continue;
            }

            if (int.TryParse(suffix, NumberStyles.None, CultureInfo.InvariantCulture, out int seq))
                maxSuffix = Math.Max(maxSuffix, seq);
        }

        return maxSuffix + 1;
    }

    private async Task<bool> JobAssignmentLookupIsCompleteAsync()
    {
        int[] requiredIds = [(int)JobAssignementTypeEnum.CurrentOwner, (int)JobAssignementTypeEnum.Responsible];
        return await _dbContext.JobAssignmentTypes.CountAsync(t => requiredIds.Contains(t.Id)) == requiredIds.Length;
    }

    private static bool IsAllAsciiDigits(ReadOnlySpan<char> span)
    {
        foreach (char c in span)
        {
            if (!char.IsAsciiDigit(c))
                return false;
        }
        return true;
    }
    #endregion
}
