using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Helpers;
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
                    c.Fax,
                    c.Email,
                    c.Website,
                    Address = c.Address != null ?
                        new AddressDto(
                            c.Address.Id,
                            (StateEnum)c.Address!.StateId!,
                            c.Address!.StateId.Value,
                            c.Address.Suburb,
                            c.Address.Street,
                            c.Address.PostCode)
                        : null,
                    jobCount = c.Jobs.Count(x => x.CouncilId == councilId && x.DeletedAt == null),
                    contactCount = c.CouncilContacts.Count(x => x.CouncilId == councilId),
                })
                .FirstOrDefaultAsync();

            if (councilData is null)
                return result.SetError(ErrorType.NotFound, $"Council not found with Id: {councilId}");

            result.Value = new CouncilDetailsDto(
                councilData.Id,
                councilData.Name,
                councilData.Phone ?? "",
                councilData.Fax,
                councilData.Email ?? "",
                councilData.Website ?? "",
                councilData.Address,
                councilData.jobCount,
                councilData.contactCount
                );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get council: {Exception}", ex.Message);
            return result.SetError(ErrorType.InternalError, "An error occured while getting the councils");
        }
    }

    /// <inheritdoc />
    public async Task<Result<int>> CreateCouncil(HttpContext httpContext, CouncilCreationDto data)
    {
        Result<int> result = new();
        try
        {
            string name = (data.CouncilName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
                return result.SetError(ErrorType.BadRequest, "Council name is required");

            Address? address = null;
            if (data.Address is not null)
            {
                address = new Address
                {
                    Street = data.Address.Street ?? "",
                    PostCode = data.Address.PostCode ?? "",
                    Suburb = data.Address.Suburb ?? "",
                    StateId = (int?)data.Address.State ?? (int)StateEnum.VIC,
                    CreatedByUserId = httpContext.UserId(),
                    Country = "AUS"
                };
                if (data.Address.LatLng is not null)
                    address.Geom = new Point(new Coordinate(data.Address.LatLng.Latitude, data.Address.LatLng.Longitude));
                await _dbContext.Addresses.AddAsync(address);
                await _dbContext.SaveChangesAsync();
            }

            Council council = new()
            {
                Name = name,
                Phone = data.Phone?.Trim(),
                Fax = data.Fax?.Trim(),
                Email = data.Email?.Trim(),
                Website = data.Website?.Trim(),
                AddressId = address?.Id,
                CreatedByUserId = httpContext.UserId(),
                CreatedOn = DateTime.UtcNow
            };
            await _dbContext.Councils.AddAsync(council);
            await _dbContext.SaveChangesAsync();
            return result.SetValue(council.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create council");
            return result.SetError(ErrorType.InternalError, "Failed to create council");
        }
    }

    /// <inheritdoc />
    public async Task<Result<CouncilDetailsDto>> UpdateCouncil(HttpContext httpContext, CouncilUpdateDto data)
    {
        Result<CouncilDetailsDto> result = new();
        try
        {
            Council? council = await _dbContext.Councils
                .Include(c => c.Address)
                .Where(c => c.Id == data.CouncilId && c.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (council is null)
                return result.SetError(ErrorType.NotFound, "Council not found");

            string name = (data.CouncilName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
                return result.SetError(ErrorType.BadRequest, "Council name is required");

            council.Name = name;
            council.Phone = data.Phone?.Trim();
            council.Fax = data.Fax?.Trim();
            council.Email = data.Email?.Trim();
            council.Website = data.Website?.Trim();
            council.ModifiedByUserId = httpContext.UserId();
            council.ModifiedOn = DateTime.UtcNow;

            if (council.Address is not null && data.Address is not null)
            {
                council.Address.Street = data.Address.Street ?? "";
                council.Address.PostCode = data.Address.PostCode ?? "";
                council.Address.Suburb = data.Address.Suburb ?? "";
                council.Address.StateId = (int?)data.Address.State ?? (int)StateEnum.VIC;
                council.Address.ModifiedByUserId = httpContext.UserId();
                council.Address.ModifiedOn = DateTime.UtcNow;
                if (data.Address.LatLng is not null)
                    council.Address.Geom = new Point(new Coordinate(data.Address.LatLng.Latitude, data.Address.LatLng.Longitude));
                else
                    council.Address.Geom = null;
            }
            else if (data.Address is not null)
            {
                Address newAddress = new()
                {
                    Street = data.Address.Street ?? "",
                    PostCode = data.Address.PostCode ?? "",
                    Suburb = data.Address.Suburb ?? "",
                    StateId = (int?)data.Address.State ?? (int)StateEnum.VIC,
                    CreatedByUserId = httpContext.UserId(),
                    Country = "AUS"
                };
                if (data.Address.LatLng is not null)
                    newAddress.Geom = new Point(new Coordinate(data.Address.LatLng.Latitude, data.Address.LatLng.Longitude));
                await _dbContext.Addresses.AddAsync(newAddress);
                await _dbContext.SaveChangesAsync();
                council.AddressId = newAddress.Id;
                council.Address = newAddress;
            }

            await _dbContext.SaveChangesAsync();
            return await GetCouncilDetails(council.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update council");
            return result.SetError(ErrorType.InternalError, "Failed to update council");
        }
    }
}
