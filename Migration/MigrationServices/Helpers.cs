namespace Migration.MigrationServices;

/// <summary>
/// Helper class providing utility methods for migration operations
/// Contains methods for address creation, date handling, and string manipulation
/// </summary>
internal static class Helpers
{
    /// <summary>
    /// Static list of state entities cached for performance
    /// </summary>
    public static List<Models.State> States = [];

    /// <summary>
    /// Creates an Address entity from source data
    /// Handles state conversion and data validation
    /// </summary>
    /// <param name="context">The database context for state lookup</param>
    /// <param name="state">The state abbreviation (e.g., "VIC", "NSW")</param>
    /// <param name="streetAddress">The street address</param>
    /// <param name="suburb">The suburb name</param>
    /// <param name="postCode">The postal code</param>
    /// <returns>A new Address entity ready for database insertion</returns>
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

    /// <summary>
    /// Converts a nullable DateTime to UTC, or returns current UTC time if null
    /// </summary>
    /// <param name="date">The nullable date to convert</param>
    /// <returns>A DateTime in UTC, or current UTC time if input is null</returns>
    public static DateTime GetValidDateWithTimezone(DateTime? date) => date.HasValue ? DateTime.SpecifyKind(date.Value, DateTimeKind.Utc) : DateTime.UtcNow;
    /// <summary>
    /// Converts a nullable DateTime to UTC, preserving null values
    /// </summary>
    /// <param name="date">The nullable date to convert</param>
    /// <returns>A nullable DateTime in UTC, or null if input is null</returns>
    public static DateTime? GetValidDateWithTimezoneNull(DateTime? date) => date.HasValue ? DateTime.SpecifyKind(date.Value, DateTimeKind.Utc) : null;

    /// <summary>
    /// Truncates a string to a maximum length, logging a warning if truncation occurs
    /// </summary>
    /// <param name="value">The string value to truncate</param>
    /// <param name="maxLength">The maximum allowed length</param>
    /// <param name="fieldName">The name of the field being truncated (for logging)</param>
    /// <param name="contactId">Optional contact ID for logging context</param>
    /// <returns>The truncated string, or empty string if input is null or empty</returns>
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
