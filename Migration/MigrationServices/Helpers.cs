namespace Migration.MigrationServices;

internal static class Helpers
{
    public static List<Models.State> States = [];

    public static Models.Address CreateAddress(PrsDbContext context, string? state, string streetAddress, string suburb, string? postCode)
    {
        try
        {
            if (States.Count is 0)
                States = [.. context.States];

            // Convert the state to the id first
            StateEnum? stateEnum = StateExtensions.FromAbbreviation(state);
            int stateId = stateEnum is not null ? (int)stateEnum : 3;
            if (!States.Select(x => x.Id).Contains(stateId))
                stateId = 3;

            Models.Address? addressObj = new()
            {
                Suburb = suburb.ToUpper(),
                StateId = stateId,
                Country = "Australia",
                Street = streetAddress,
                PostCode = postCode ?? "3000",
                CreatedByUserId = 95
            };
            return addressObj;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public static DateTime GetValidDateWithTimezone(DateTime? date) => date.HasValue ? DateTime.SpecifyKind(date.Value, DateTimeKind.Utc) : DateTime.UtcNow;
    public static DateTime? GetValidDateWithTimezoneNull(DateTime? date) => date.HasValue ? DateTime.SpecifyKind(date.Value, DateTimeKind.Utc) : null;

    public static string TruncateString(string? value, int maxLength, string fieldName = "", int? contactId = null)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Length > maxLength)
        {
            Console.WriteLine($"WARNING: Truncating {fieldName} for contact {contactId}: '{value}' (length: {value.Length}) to {maxLength} chars");
            return value[..maxLength];
        }

        return value;
    }

}
