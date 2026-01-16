using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;
using Quartz.Util;

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
    public async Task<Result<PagedResponse<ListJobDto>>> GetAllJobs(int page, int pageSize, SortDirectionEnum? order, string? searchFilter, string? orderby)
    {
        Result<PagedResponse<ListJobDto>> result = new();
        try
        {
            IQueryable<Job> jobQuery = _dbContext.Jobs
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .AsQueryable();

            if (!searchFilter.IsNullOrWhiteSpace())
            {
                searchFilter = searchFilter!.Trim();
                bool isNumeric = int.TryParse(searchFilter, out int numericValue);
                jobQuery = jobQuery.Where(job =>
                            (isNumeric && job.Id == numericValue)
                            || (isNumeric && job.JobNumber != null && job.JobNumber.Value == numericValue)
                            || job.Address != null && job.Address.SearchVector != null && job.Address.SearchVector.Matches(searchFilter)
                            || job.Contact.SearchVector != null && job.Contact.SearchVector.Matches(searchFilter));
            }
            string stringQUery = jobQuery.ToQueryString();
            bool isDescending = order is SortDirectionEnum.Desc;
            jobQuery = orderby switch
            {
                nameof(ListJobDto.JobId) => isDescending
                    ? jobQuery.OrderByDescending(x => x.Id)
                    : jobQuery.OrderBy(x => x.Id),
                nameof(ListJobDto.Contact1.fullName) => isDescending
                    ? jobQuery.OrderByDescending(x => x.Contact.FullName)
                    : jobQuery.OrderBy(x => x.Contact.FullName),
                nameof(ListJobDto.JobNumber) => isDescending
                    ? jobQuery.OrderByDescending(x => x.JobNumber)
                    : jobQuery.OrderBy(x => x.JobNumber),
                // Address sub-properties - EF Core can handle null navigation properties
                $"{nameof(ListJobDto.Address)}.{nameof(ListJobDto.Address.suburb)}" => isDescending
                    ? jobQuery.OrderByDescending(x => x.Address!.Suburb)
                    : jobQuery.OrderBy(x => x.Address!.Suburb),
                $"{nameof(ListJobDto.Address)}.{nameof(ListJobDto.Address.street)}" => isDescending
                    ? jobQuery.OrderByDescending(x => x.Address!.Street)
                    : jobQuery.OrderBy(x => x.Address!.Street),
                $"{nameof(ListJobDto.Address)}.{nameof(ListJobDto.Address.postCode)}" => isDescending
                    ? jobQuery.OrderByDescending(x => x.Address!.PostCode)
                    : jobQuery.OrderBy(x => x.Address!.PostCode),
                _ => jobQuery.OrderByDescending(x => x.Id) // Default ordering by JobId
            };

            int skipValue = (page - 1) * pageSize;
            List<ListJobDto> jobs = await jobQuery
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
                            ))
                        .ToListAsync();
            int total = await jobQuery.CountAsync();
            // Create the paged response
            PagedResponse<ListJobDto> pagedResponse = new(jobs, pageSize, page, total);
            result.Value = pagedResponse;
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
            .Where(j => j.Id == jobId && j.DeletedAt == null)
            .Select(x => new JobDetailsDto
            {
                JobId = x.Id,
                JobNumber = x.JobNumber ?? 0,
                JobType = (JobTypeEnum)x.JobTypeId,
                Address = new AddressDTO(
                    x.AddressId ?? 1,
                    (StateEnum)x.Address!.StateId!,
                    x.Address.StateId ?? 3,
                    x.Address.Suburb,
                    x.Address.Street,
                    x.Address.PostCode
                ),
                Colour = x.JobColour != null
                    ? new JobColourDto(x.JobColourId!.Value, x.JobColour.Color)
                    : null,
                Contact = x.Contact != null
                    ? new JobContactDto(
                        x.ContactId,
                        x.Contact.FullName,
                        x.Contact.Email,
                        x.Contact.Phone ?? ""
                    )
                    : null,
                Council = x.Council != null ? new JobCouncilDto(x.Council.Id, x.Council.Name) : null,
            })
            .FirstOrDefaultAsync();

        if (job is null)
            return result.SetError(ErrorType.NotFound, "Invalid job Id");

        job.Notes = await _dbContext.JobNotes
            .AsNoTracking()
            .Where(n => n.JobId == jobId && n.DeletedAt == null)
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
            });
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

        result.Value = job;
        return result;
    }
}
