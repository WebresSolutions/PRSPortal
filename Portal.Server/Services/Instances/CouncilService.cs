using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Councils;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Instances;

public class CouncilService(PrsDbContext _dbContext, ILogger<CouncilService> _logger) : ICouncilService
{
    public async Task<Result<CouncilPartialDto[]>> GetCouncils()
    {
        Result<CouncilPartialDto[]> result = new();
        try
        {
            result.Value = await _dbContext.Councils
                .OrderBy(c => c.Name)
                .Select(c => new CouncilPartialDto(
                    c.Id,
                    c.Name,
                    c.Phone ?? "",
                    c.Email ?? "",
                    c.Website ?? ""))
                .ToArrayAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get councils: {Exception}", ex.Message);
            return result.SetError(ErrorType.InternalError, "An error occured while getting the councils");
        }
    }

    public async Task<Result<CouncilDetailsDto>> GetCouncilDetails(int councilId)
    {
        Result<CouncilDetailsDto> result = new();
        try
        {
            var councilData = await _dbContext.Councils
                .Where(c => c.Id == councilId)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Phone,
                    c.Email,
                    c.Website,
                    Address = c.Address != null ?
                        new AddressDTO(
                            c.Address.Id,
                            (StateEnum)c.Address!.StateId!,
                            c.Address!.StateId.Value,
                            c.Address.Suburb,
                            c.Address.Street,
                            c.Address.PostCode)
                        : null
                })
                .FirstOrDefaultAsync();

            if (councilData is null)
                return result.SetError(ErrorType.NotFound, $"Council not found with Id: {councilId}");

            // Return council details without jobs (jobs loaded separately)
            result.Value = new CouncilDetailsDto(
                councilData.Id,
                councilData.Name,
                councilData.Phone ?? "",
                councilData.Email ?? "",
                councilData.Website ?? "",
                councilData.Address,
                new List<ListJobDto>()); // Empty list - jobs loaded via separate endpoint

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get council: {Exception}", ex.Message);
            return result.SetError(ErrorType.InternalError, "An error occured while getting the councils");
        }
    }

    public async Task<Result<PagedResponse<ListJobDto>>> GetCouncilJobs(int councilId, int page, int pageSize, SortDirectionEnum? order, string? orderby)
    {
        Result<PagedResponse<ListJobDto>> result = new();
        try
        {
            IQueryable<Data.Models.Job> jobQuery = _dbContext.Jobs
                .AsNoTracking()
                .Where(j => j.CouncilId == councilId && j.DeletedAt == null)
                .AsQueryable();

            bool isDescending = order is SortDirectionEnum.Desc;
            jobQuery = orderby switch
            {
                nameof(ListJobDto.JobId) => isDescending
                    ? jobQuery.OrderByDescending(x => x.Id)
                    : jobQuery.OrderBy(x => x.Id),
                nameof(ListJobDto.Contact1) + "." + nameof(ContactDto.fullName) => isDescending
                    ? jobQuery.OrderByDescending(x => x.Contact.FullName)
                    : jobQuery.OrderBy(x => x.Contact.FullName),
                nameof(ListJobDto.JobNumber) => isDescending
                    ? jobQuery.OrderByDescending(x => x.JobNumber)
                    : jobQuery.OrderBy(x => x.JobNumber),
                $"{nameof(ListJobDto.Address)}.{nameof(AddressDTO.suburb)}" => isDescending
                    ? jobQuery.OrderByDescending(x => x.Address!.Suburb)
                    : jobQuery.OrderBy(x => x.Address!.Suburb),
                $"{nameof(ListJobDto.Address)}.{nameof(AddressDTO.street)}" => isDescending
                    ? jobQuery.OrderByDescending(x => x.Address!.Street)
                    : jobQuery.OrderBy(x => x.Address!.Street),
                $"{nameof(ListJobDto.Address)}.{nameof(AddressDTO.postCode)}" => isDescending
                    ? jobQuery.OrderByDescending(x => x.Address!.PostCode)
                    : jobQuery.OrderBy(x => x.Address!.PostCode),
                _ => jobQuery.OrderByDescending(x => x.Id) // Default ordering by JobId descending
            };

            int skipValue = (page - 1) * pageSize;
            List<ListJobDto> jobs = await jobQuery
                .Skip(skipValue)
                .Take(pageSize)
                .Select(j => new ListJobDto(
                    j.Id,
                    new AddressDTO(j.AddressId!.Value, (StateEnum)j.Address!.StateId!, j.Address!.StateId.Value, j.Address.Suburb, j.Address.Street, j.Address.PostCode),
                    j.Contact != null ? new ContactDto(j.ContactId, j.Contact.FullName) : null,
                    j.Contact != null && j.Contact.ParentContact != null ? new ContactDto(j.Contact.ParentContactId ?? 0, j.Contact.ParentContact!.FullName) : null,
                    j.JobNumber,
                    j.JobType.Name,
                    j.JobType.Id))
                .ToListAsync();

            int total = await jobQuery.CountAsync();

            // Create the paged response
            PagedResponse<ListJobDto> pagedResponse = new(jobs, pageSize, page, total);
            result.Value = pagedResponse;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get council jobs: {Exception}", ex.Message);
            return result.SetError(ErrorType.InternalError, "An error occured while getting the council jobs");
        }
    }
}
