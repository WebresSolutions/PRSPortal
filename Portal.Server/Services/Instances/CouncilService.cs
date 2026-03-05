using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Councils;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Instances;

public class CouncilService(PrsDbContext _dbContext, ILogger<CouncilService> _logger) : ICouncilService
{
    /// <summary>
    /// Asynchronously retrieves all councils, returning basic information for each council.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/> object
    /// with an array of <see cref="CouncilPartialDto"/> instances representing the councils. If no councils are found,
    /// the array is empty. If an error occurs, the result contains error information.</returns>
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

    /// <summary>
    /// Retrieves detailed information for a specific council by its unique identifier.
    /// </summary>
    /// <remarks>The returned details do not include job information; jobs are loaded via a separate endpoint.
    /// Returns an error result with <see cref="ErrorType.NotFound"/> if no council exists with the specified
    /// ID.</remarks>
    /// <param name="councilId">The unique identifier of the council to retrieve details for. Must be a valid council ID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{CouncilDetailsDto}"/> object with the council details if found; otherwise, an error result
    /// indicating the reason for failure.</returns>
    public async Task<Result<CouncilDetailsDto>> GetCouncilDetails(int councilId)
    {
        Result<CouncilDetailsDto> result = new();
        try
        {
            var councilData = await _dbContext.Councils
                .AsSplitQuery()
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
                        : null,
                    jobCount = c.Jobs.Count(x => x.CouncilId == councilId),
                    contactCount = c.CouncilContacts.Count(x => x.CouncilId == councilId),
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
                councilData.jobCount,
                councilData.contactCount
                ); // Empty list - jobs loaded via separate endpoint

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get council: {Exception}", ex.Message);
            return result.SetError(ErrorType.InternalError, "An error occured while getting the councils");
        }
    }
}
