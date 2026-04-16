using Portal.Shared.DTO.Quote;

namespace Portal.Server.Services.Interfaces.UtilityServices;

public interface IEmailService
{
    /// <summary>
    /// Sends the quote acceptance email with HTML body, optional PDF attachments, and a portal link to accept the fee proposal.
    /// </summary>
    /// <remarks>If a global override email address is configured, all outgoing emails will be sent to that
    /// address regardless of the specified recipients. Errors encountered during sending are logged but do not affect
    /// the return value.</remarks>
    /// <param name="to">An array of recipient email addresses to which the message will be sent. If an override address is configured,
    /// all emails will be sent to that address instead.</param>
    /// <param name="subject">The subject line of the email message.</param>
    /// <param name="details">Quote details used to render the email body.</param>
    /// <param name="attachments">An array of attachments to include with the email, where each attachment consists of the file content as a byte
    /// array and the file name. Can be empty if no attachments are needed.</param>
    /// <param name="portalBaseUrl">Absolute base URL of the Blazor client (e.g. https://app.example.com), used to build the quote acceptance link.</param>
    /// <param name="acceptanceToken">Optional token appended to the acceptance URL when present.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the method
    /// completes execution.</returns>
    Task<bool> SendQuoteEmail(string[] to, string subject, QuoteDetailsDto details, (byte[] content, string fileName)[] attachments, string? acceptanceToken = null);
}
