using Portal.Shared.DTO.Quote;

namespace Portal.Server.Services.Interfaces.UtilityServices;

public interface IEmailService
{
    /// <summary>
    /// Sends an email message to the specified recipients with the given subject, body, and optional attachments.
    /// </summary>
    /// <remarks>If a global override email address is configured, all outgoing emails will be sent to that
    /// address regardless of the specified recipients. Errors encountered during sending are logged but do not affect
    /// the return value.</remarks>
    /// <param name="to">An array of recipient email addresses to which the message will be sent. If an override address is configured,
    /// all emails will be sent to that address instead.</param>
    /// <param name="subject">The subject line of the email message.</param>
    /// <param name="body">The body content of the email message.</param>
    /// <param name="attachments">An array of attachments to include with the email, where each attachment consists of the file content as a byte
    /// array and the file name. Can be empty if no attachments are needed.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the method
    /// completes execution.</returns>
    Task<bool> SendQuoteEmail(string[] to, string subject, QuoteDetailsDto details, (byte[] content, string fileName)[] attachments);
}
