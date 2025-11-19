using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Instances;

public class JobService(PrsDbContext _dbContext, ILogger<JobService> _logger) : IJobService
{
    public async Task<Result<PagedResponse<ListJobDto>>> GetAllJobs(int page, int pageSize, SortDirectionEnum? order, string? nameFilter, string? orderby)
    {
        Result<PagedResponse<ListJobDto>> result = new();
        try
        {
            IQueryable<Job> jobQuery = _dbContext.Jobs
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .AsQueryable();

            if (nameFilter is not null)
            {
                jobQuery = jobQuery.Where(x =>
                        (x.Address != null && x.Address.Street.Contains(nameFilter))
                        || (x.Address != null && x.Address.Suburb.Contains(nameFilter))
                        || (x.Contact.FirstName.Contains(nameFilter))
                        || (x.Contact.LastName.Contains(nameFilter))
                        || (x.ConstructionNumber != null && x.ConstructionNumber.ToString().Contains(nameFilter))
                        || (x.SurveyNumber != null && x.SurveyNumber.ToString().Contains(nameFilter))
                        || (x.Id.ToString().Contains(nameFilter))
                        );
            }
            bool isDescending = order is SortDirectionEnum.Desc;
            jobQuery = orderby switch
            {
                nameof(ListJobDto.JobId) => isDescending ? jobQuery.OrderByDescending(x => x.Id) : jobQuery.OrderBy(x => x.Id),
                nameof(ListJobDto.AddressId) => isDescending ? jobQuery.OrderByDescending(x => x.AddressId) : jobQuery.OrderBy(x => x.AddressId),
                nameof(ListJobDto.Contact) => isDescending ? jobQuery.OrderByDescending(x => x.Contact) : jobQuery.OrderBy(x => x.Contact),
                nameof(ListJobDto.ContactId) => isDescending ? jobQuery.OrderByDescending(x => x.ContactId) : jobQuery.OrderBy(x => x.ContactId),
                nameof(ListJobDto.ConstructionNumber) => isDescending ? jobQuery.OrderByDescending(x => x.ConstructionNumber) : jobQuery.OrderBy(x => x.ConstructionNumber),
                nameof(ListJobDto.SurveyNumber) => isDescending ? jobQuery.OrderByDescending(x => x.SurveyNumber) : jobQuery.OrderBy(x => x.SurveyNumber),
                // Address sub-properties - EF Core can handle null navigation properties
                "Address.Suburb" => isDescending
                    ? jobQuery.OrderByDescending(x => x.Address.Suburb)
                    : jobQuery.OrderBy(x => x.Address.Suburb),
                "Address.Street" => isDescending
                    ? jobQuery.OrderByDescending(x => x.Address.Street)
                    : jobQuery.OrderBy(x => x.Address.Street),
                "Address.PostCode" => isDescending
                    ? jobQuery.OrderByDescending(x => x.Address.PostCode)
                    : jobQuery.OrderBy(x => x.Address.PostCode),
                _ => jobQuery.OrderBy(x => x.Id) // Default ordering by JobId
            };

            int skipValue = (page - 1) * pageSize;
            List<ListJobDto> jobs = await jobQuery
                        .Skip(skipValue)
                        .Take(pageSize)
                        .Select(x => new ListJobDto(
                            x.Id,
                            new AddressDTO(x.AddressId ?? 1, (StateEnum)x.Address.StateId, x.Address.StateId ?? 3, x.Address.Suburb, x.Address.Street, x.Address.PostCode),
                            x.AddressId ?? 0,
                            x.ContactId,
                            $"{x.Contact.FirstName} {x.Contact.LastName}",
                            x.SurveyNumber,
                            x.ConstructionNumber
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
