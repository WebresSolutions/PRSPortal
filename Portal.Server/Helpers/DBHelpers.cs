using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Data.Models;

namespace Portal.Server.Helpers;

public static class DBHelpers
{

    /// <summary>
    /// Creates a new address or updates an existing address in the database asynchronously.
    /// </summary>
    /// <param name="context">The database context used to access and modify address records. Cannot be null.</param>
    /// <param name="address">The address entity to create or update. If the address has an Id of 0, a new address is created; otherwise, the
    /// existing address with the specified Id is updated. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created or updated address
    /// entity.</returns>
    /// <exception cref="Exception">Thrown if no address with the specified Id exists in the database when attempting to update.</exception>
    public static async Task<Address> CreateOrUpdateAddress(PrsDbContext context, Address address, int userId)
    {
        // Create a new one
        if (address.Id is 0)
        {
            await context.Addresses.AddAsync(address);
            await context.SaveChangesAsync();
            return address;
        }
        else
        {
            Address existingAddress = await context.Addresses.FirstOrDefaultAsync(a => a.Id == address.Id)
                ?? throw new Exception("Address not found.");

            existingAddress.Street = address.Street;
            existingAddress.Suburb = address.Suburb;
            existingAddress.StateId = address.StateId;
            existingAddress.PostCode = address.PostCode;
            existingAddress.StateId = address.StateId;
            existingAddress.Country = address.Country;
            existingAddress.ModifiedByUserId = userId;
            existingAddress.ModifiedOn = address.ModifiedOn;
            existingAddress.Geohash = address.Geohash;

            if (address.Geom is not null)
                existingAddress.Geom = new NetTopologySuite.Geometries.Point(address.Geom.Coordinate);

            await context.SaveChangesAsync();
            return existingAddress;
        }
    }
}
