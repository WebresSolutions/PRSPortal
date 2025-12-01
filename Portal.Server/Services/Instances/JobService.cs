using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;
using Quartz.Util;

namespace Portal.Server.Services.Instances;

public class JobService(PrsDbContext _dbContext, ILogger<JobService> _logger) : IJobService
{
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
            string? addressSuburb = nameof(ListJobDto.Address.suburb);
            Console.WriteLine(addressSuburb);

            bool isDescending = order is SortDirectionEnum.Desc;
            jobQuery = orderby switch
            {
                nameof(ListJobDto.JobId) => isDescending
                    ? jobQuery.OrderByDescending(x => x.Id)
                    : jobQuery.OrderBy(x => x.Id),
                nameof(ListJobDto.Contact) => isDescending
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
                            x.AddressId ?? 0,
                            x.ContactId,
                            x.Contact.FullName,
                            x.JobNumber,
                            x.JobType.Name
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
}
