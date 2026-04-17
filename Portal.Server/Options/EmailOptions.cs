namespace Portal.Server.Options;

public class EmailOptions
{
    public required string ApiKey { get; set; }
    public required string FromEmailAddress { get; set; }
    public string? ToEmailAddressOverride { get; set; }
    public bool SendEmail { get; set; }

    /// <summary>
    /// Optional absolute base URL of the client app (no trailing slash), e.g. https://portal.prs.au.
    /// When set, quote acceptance links in emails use this host instead of the server request host.
    /// </summary>
    public required string BaseUrl { get; set; }
}
