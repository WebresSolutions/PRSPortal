using Portal.Data;
using Portal.Data.Models;
using Portal.Shared;

namespace Migration.MigrationServices;

internal static class Helpers
{
    public static Address FindOrCreateAddress(PrsDbContext dbContext, string? state, string streetAddress, string suburb, string? postCode)
    {
        // Convert the state to the id first
        StateEnum? stateEnum = StateExtensions.FromAbbreviation(state);
        stateEnum ??= StateEnum.VIC;

        Address? addressObj = new()
        {
            City = suburb.ToUpper(),
            StateId = (int)stateEnum,
            Country = "Australia",
            Street = streetAddress,
            PostCode = postCode ?? "3000",
            CreatedByUserId = 95
        };
        dbContext.Addresses.Add(addressObj);
        return addressObj;
    }
}
