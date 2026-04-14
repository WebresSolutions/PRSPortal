namespace Portal.Server.Options;

public class EmailOptions
{
    public required string ApiKey { get; set; }
    public required string FromEmailAddress { get; set; }
    public string? ToEmailAddressOverride { get; set; }
}
