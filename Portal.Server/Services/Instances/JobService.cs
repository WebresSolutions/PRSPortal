using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;


namespace Portal.Server.Services.Instances;

/// <summary>
/// Service implementation for job-related business logic
/// Handles job retrieval, filtering, and data transformation
/// </summary>
public class JobService(PrsDbContext _dbContext, ILogger<JobService> _logger) : IJobService
{
    /// <summary>
    /// Retrieves a paged list of jobs with optional filtering and sorting
    /// </summary>
    /// <param name="page">The page number to retrieve (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="order">The sort direction (ascending or descending)</param>
    /// <param name="searchFilter">Optional search filter for job names, addresses, or job numbers</param>
    /// <param name="orderby">Optional field name to sort by</param>
    /// <returns>A result containing a paged response of job DTOs</returns>
    public async Task<Result<PagedResponse<ListJobDto>>> GetAllJobs(
        int page,
        int pageSize,
        SortDirectionEnum? order,
        string? addressSearch,
        string? contactSearch,
        string? jobNumberSearch,
        string? orderby,
        bool deleted = false)
    {
        Result<PagedResponse<ListJobDto>> result = new();
        try
        {
            // 1. Base query with the Deleted filter already applied
            IQueryable<Job> query = _dbContext.Jobs
                .AsNoTracking();

            query = !deleted ? query.Where(x => x.DeletedAt == null) : query.Where(x => x.DeletedAt != null);

            if (!string.IsNullOrWhiteSpace(addressSearch))
                query = query.Where(job => job.Address != null &&
                                           job.Address.SearchVector.Matches(addressSearch));

            if (!string.IsNullOrWhiteSpace(contactSearch))
                query = query.Where(job => job.Contact != null &&
                                           job.Contact.SearchVector.Matches(contactSearch));

            if (!string.IsNullOrWhiteSpace(jobNumberSearch))
                if (int.TryParse(jobNumberSearch, out int num))
                    query = query.Where(job => job.JobNumber != null && job.JobNumber.Value == num);


            bool isDescending = order is SortDirectionEnum.Desc;
            query = orderby switch
            {
                nameof(ListJobDto.JobId) => isDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                nameof(ListJobDto.Contact1.fullName) => isDescending ? query.OrderByDescending(x => x.Contact.FullName) : query.OrderBy(x => x.Contact.FullName),
                nameof(ListJobDto.JobNumber) => isDescending ? query.OrderByDescending(x => x.JobNumber) : query.OrderBy(x => x.JobNumber),
                $"{nameof(ListJobDto.Address)}.{nameof(ListJobDto.Address.Suburb)}" => isDescending ? query.OrderByDescending(x => x.Address!.Suburb) : query.OrderBy(x => x.Address!.Suburb),
                $"{nameof(ListJobDto.Address)}.{nameof(ListJobDto.Address.Street)}" => isDescending ? query.OrderByDescending(x => x.Address!.Street) : query.OrderBy(x => x.Address!.Street),
                $"{nameof(ListJobDto.Address)}.{nameof(ListJobDto.Address.PostCode)}" => isDescending ? query.OrderByDescending(x => x.Address!.PostCode) : query.OrderBy(x => x.Address!.PostCode),
                _ => query.OrderByDescending(x => x.Id)
            };

            int total = await query.CountAsync(); // Note: Count the union-ed set
            int skipValue = (page - 1) * pageSize;

            IQueryable<ListJobDto> jobs = query
                .Skip(skipValue)
                .Take(pageSize)
                .Select(x => new ListJobDto(
                    x.Id,
                    new AddressDTO(x.AddressId ?? 1, (StateEnum)x.Address!.StateId!, x.Address.StateId ?? 3, x.Address.Suburb, x.Address.Street, x.Address.PostCode),
                    x.Contact != null ? new ContactDto(x.ContactId, x.Contact.FullName) : null,
                    x.Contact != null && x.Contact.ParentContact != null ? new ContactDto(x.Contact.ParentContactId ?? 0, x.Contact.ParentContact!.FullName) : null,
                    x.JobNumber,
                    x.JobType.Name,
                    x.JobTypeId
                ));
            string querystring = jobs.ToQueryString();

            // Materialize the query
            List<ListJobDto> jobsList = await jobs.ToListAsync();

            result.Value = new PagedResponse<ListJobDto>(jobsList, pageSize, page, total);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all jobs");
            return result.SetError(ErrorType.InternalError, "Failed to get list of jobs");
        }
    }

    /// <summary>
    /// Retrieves detailed information for a job specified by its unique identifier.
    /// </summary>
    /// <remarks>The returned job details include associated notes and address information. If the specified
    /// job does not exist or has been deleted, the result will indicate a 'NotFound' error.</remarks>
    /// <param name="jobId">The unique identifier of the job to retrieve. Must refer to an existing, non-deleted job.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{JobDetailsDto}"/> with job details if found; otherwise, an error indicating that the job was not
    /// found.</returns>
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
                JobNumber = x.JobNumber ?? 0,
                JobType = (JobTypeEnum)x.JobTypeId,
                Address = x.Address != null ? new AddressDTO(
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
                Details = x.Details,
                PrimaryContact = x.Contact != null
                    ? new JobContactDto(
                        x.ContactId,
                        x.Contact.FullName,
                        x.Contact.Email,
                        x.Contact.Phone ?? ""
                    )
                    : null,
                TechnicalContacts = x.TechnicalContacts
                    .Select(tc
                        => new TechnicalContactDto(tc.Id, tc.ContactId, tc.JobId, tc.Type.Name, tc.Contact.FullName, tc.Contact.Email, tc.Contact.Phone, false))
                    .ToList(),
                TimeSheets = x.TimesheetEntries
                    .Select(ts
                        => new TimeSheetDto(ts.Id, ts.TypeId, ts.DateFrom, ts.DateTo, ts.UserId, ts.JobId, ts.Description, ts.User.DisplayName))
                    .ToList(),
                ContactId = x.ContactId,
                Council = x.Council != null ? new JobCouncilDto(x.Council.Id, x.Council.Name) : null,
                CouncilId = x.CouncilId,
                LastModified = x.ModifiedByUserId != null && x.ModifiedOn != null ? new LastModifiedDto(x.ModifiedOn.Value, x.ModifiedByUser!.DisplayName) : null
            })
            .FirstOrDefaultAsync();

        if (job is null)
            return result.SetError(ErrorType.NotFound, "Invalid job Id");

        job.Notes = [];

        // Get the job site visits from the schedules table
        IQueryable<JobSiteVisitsDto> query = _dbContext.Schedules
            .AsNoTracking()
            .AsSplitQuery()
            .Where(s => s.JobId == jobId)
            .Select(s => new JobSiteVisitsDto
            {
                ScheduleId = s.Id,
                Assignees = s.ScheduleTrack.ScheduleUsers
                    .Select(su => su.User.DisplayName)
                    .ToArray(),
                Start = s.StartTime,
                End = s.EndTime,
                Category = s.ScheduleTrack.JobType.Name,
                Notes = s.Notes ?? string.Empty
            })
            .OrderByDescending(x => x.Start);
        job.SiteVisits = await query.ToListAsync();

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
            .ToListAsync();

        return result.SetValue(job);
    }

    /// <summary>
    /// Creates a new job
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task<Result<int>> CreateJob(HttpContext httpContext, JobCreationDto data)
    {
        Result<int> result = new();
        try
        {
            // Validation 
            if (await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == data.ContactId) is null)
                return result.SetError(ErrorType.BadRequest, "Invalid contact id supplied");

            if (data.CouncilId is not null && await _dbContext.Councils.FirstOrDefaultAsync(x => x.Id == data.CouncilId) is null)
                return result.SetError(ErrorType.BadRequest, "Invalid council id supplied");

            if (data.JobColourId is not null && await _dbContext.JobColours.FirstOrDefaultAsync(x => x.Id == data.JobColourId) is null)
                return result.SetError(ErrorType.BadRequest, "Invalid job colour id supplied");

            if (_dbContext.Jobs.Any(j => j.JobNumber == data.JobNumber) || data.JobNumber == 0)
                return result.SetError(ErrorType.BadRequest, "Job number already exists or invalid");

            Job job = new()
            {
                ContactId = data.ContactId,
                CouncilId = data.CouncilId,
                JobColourId = data.JobColourId,
                Details = data.Details,
                CreatedByUserId = httpContext.UserId(),
                CreatedOn = DateTime.UtcNow,
                JobTypeId = (int)data.JobType,
                JobNumber = data.JobNumber
            };

            if (data.Address is not null)
            {
                Address address = new()
                {
                    Street = data.Address.Street,
                    PostCode = data.Address.PostCode,
                    Suburb = data.Address.Suburb,
                    StateId = (int?)data.Address.State ?? (int)StateEnum.VIC,
                    CreatedByUserId = httpContext.UserId(),
                    Country = "AUS"
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
            // Create the other objects first

            await _dbContext.Jobs.AddAsync(job);
            await _dbContext.SaveChangesAsync();

            return result.SetValue(job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create job");
            return result.SetError(ErrorType.InternalError, "Failed to create job");
        }
    }

    /// <summary>
    /// Updates a job 
    /// </summary>
    /// <param name="httpContext">The context of the user updating the job</param>
    /// <param name="updateJobDto"></param>
    /// <returns></returns>
    public async Task<Result<JobDetailsDto>> UpdateJob(HttpContext httpContext, JobDetailsDto updateJobDto)
    {
        Result<JobDetailsDto> result = new();
        try
        {
            Job? job = await _dbContext.Jobs
                .Include(j => j.Address)
                .Where(j => j.Id == updateJobDto.JobId && j.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (job is null)
                return result.SetError(ErrorType.NotFound, "Invalid job Id");

            job.JobTypeId = (int)updateJobDto.JobType;
            job.JobColourId = updateJobDto.JobColourId;
            job.ModifiedByUserId = httpContext.UserId();
            job.Details = updateJobDto.Details;

            // Enforce uniqueness of job number if it has changed
            if (job.JobNumber != updateJobDto.JobNumber)
            {
                if (_dbContext.Jobs.Any(j => j.JobNumber == updateJobDto.JobNumber && j.DeletedAt == null))
                    return result.SetError(ErrorType.Conflict, "Job number already exists");

                job.JobNumber = updateJobDto.JobNumber;
            }

            if (updateJobDto.ContactId != job.ContactId)
            {
                Contact? contact = await _dbContext.Contacts.FirstOrDefaultAsync(c => c.Id == updateJobDto.ContactId);
                if (contact is null)
                    return result.SetError(ErrorType.BadRequest, "Invalid Contact Id");

                job.ContactId = updateJobDto.ContactId;
                job.Contact = contact;
            }

            if (updateJobDto.CouncilId != job.CouncilId)
            {
                Council? council = await _dbContext.Councils.FirstOrDefaultAsync(c => c.Id == updateJobDto.CouncilId);
                if (council is null)
                    return result.SetError(ErrorType.BadRequest, "Invalid Contact Id");

                job.CouncilId = updateJobDto.CouncilId;
                job.Council = council;
            }


            if (job.Address is not null && updateJobDto.Address is not null)
            {
                job.Address.Street = updateJobDto.Address.Street;
                job.Address.PostCode = updateJobDto.Address.PostCode;
                job.Address.Suburb = updateJobDto.Address.Suburb;
                job.Address.StateId = updateJobDto.Address.State is null ? (int)StateEnum.VIC : (int)updateJobDto.Address.State;
                job.Address.ModifiedByUserId = httpContext.UserId();

                if (updateJobDto.Address.LatLng is null)
                    job.Address.Geom = null;
                else if (updateJobDto.Address.LatLng is LatLngDto latLng)
                    job.Address.Geom = new(new Coordinate(latLng.Latitude, latLng.Longitude));
            }
            else
            {
                if (updateJobDto.Address is not null)
                {
                    job.Address = new Address
                    {
                        Street = updateJobDto.Address.Street,
                        PostCode = updateJobDto.Address.PostCode,
                        Suburb = updateJobDto.Address.Suburb,
                        StateId = updateJobDto.Address.State is null ? (int)StateEnum.VIC : (int)updateJobDto.Address.State,
                        CreatedByUserId = httpContext.UserId(),
                        Country = "AUS"
                    };

                    if (updateJobDto.Address.LatLng is not null)
                    {
                        Point latlng = new(new Coordinate(updateJobDto.Address.LatLng.Latitude, updateJobDto.Address.LatLng.Longitude));
                        job.Address.Geom = latlng;
                    }

                    await _dbContext.AddAsync(job.Address);
                    await _dbContext.SaveChangesAsync();
                    job.AddressId = job.Address.Id;
                }
            }

            await _dbContext.SaveChangesAsync();
            // Return the updated job details
            return await GetJob(updateJobDto.JobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update job with ID {JobId}", updateJobDto.JobId);
            return result.SetError(ErrorType.InternalError, "Failed to update job");
        }
    }

    /// <summary>
    /// Sets the deleted flag on a job
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="id">The job being deleted</param>
    /// <returns></returns>
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

    /// <summary>
    /// Creates a new note associated with a job and returns the updated job details.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the current request. Used to access user information and request metadata. Cannot be null.</param>
    /// <param name="note">The note to add to the job. Must contain valid job and note information. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the updated job
    /// details if the note is created successfully; otherwise, contains error information.</returns>
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

    /// <summary>
    /// Updates a note, this can be used to update the content, action required, assigned user or deleted status of a note
    /// </summary>
    /// <param name="httpContext">The httpcontext</param>
    /// <param name="note">The note to update</param>
    /// <returns>The job details</returns>
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

    /// <summary>
    /// Gets all users assigend job notes   
    /// </summary>
    /// <param name="httpContext">The http context</param>
    /// <param name="userId">The user ID</param>
    /// <param name="includeDeleted">If should include deleted. </param>
    /// <returns>A list of job notes assigned to the specific users </returns>
    /// <summary>
    /// Gets the notes for a job by job id.
    /// </summary>
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

    public async Task<Result<List<JobNoteDto>>> GetUserAssignedJobsNotes(HttpContext httpContext, int userId, bool includeDeleted)
    {
        Result<List<JobNoteDto>> result = new();

        try
        {
            if (userId is 0)
            {
                // Get the calling user
                userId = httpContext.UserId();
            }

            result.Value = await _dbContext.JobNotes
                .Where(x => x.AssignedUserId == userId &&
                    (includeDeleted || x.DeletedAt == null)
                )
                .Select(n => new JobNoteDto
                {
                    NoteId = n.Id,
                    Content = n.Note,
                    AssignedUser = n.AssignedUserId != null
                    ? new(n.AssignedUserId.Value, n.AssignedUser!.DisplayName ?? "")
                    : null,
                    DateCreated = n.CreatedOn
                })
                .OrderByDescending(n => n.DateCreated)
                .ToListAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user assigned job notes");
            return result.SetError(ErrorType.InternalError, "Failed to get user assigned job notes");
        }
    }
}
