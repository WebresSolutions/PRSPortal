using Microsoft.Extensions.Options;
using Portal.Server.Options;
using Portal.Server.Services.Interfaces.UtilityServices;
using Portal.Server.Views.ViewModels;
using Portal.Shared.DTO.Quote;
using Smtp2Go.Api;
using Smtp2Go.Api.Models.Emails;

namespace Portal.Server.Services.Instances.UtilityServices;

public class EmailService(
    IOptions<EmailOptions> _IEmailOptions,
    IApiService _smtp2GoService,
    ILogger<EmailService> _logger,
    IRazorViewRenderer _IViewRenderer,
    IWebHostEnvironment _environment) : IEmailService
{
    EmailOptions _EmailOptions = _IEmailOptions.Value;

    ///<inheritdoc  />
    public async Task<bool> SendQuoteEmail(string[] to, string subject, QuoteDetailsDto details, (byte[] content, string fileName)[] attachments, string? acceptanceToken = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(_EmailOptions.ToEmailAddressOverride))
                to = [_EmailOptions.ToEmailAddressOverride];

            string baseUrl = _EmailOptions.BaseUrl;
            string acceptPath = $"/quotes/accept/{details.Id}";
            string acceptUrl = string.IsNullOrEmpty(acceptanceToken)
                ? $"{baseUrl}{acceptPath}"
                : $"{baseUrl}{acceptPath}?token={Uri.EscapeDataString(acceptanceToken)}";

            QuoteAcceptanceModel model = new()
            {
                AcceptQuoteUrl = acceptUrl,
                Token = acceptanceToken ?? string.Empty,
                Details = details,
            };
            string view = "Views/Emails/QuoteAcceptanceEmail.cshtml";
            string emailBody = await _IViewRenderer.Render(view, model);

            EmailMessage message = new("From Name <from@address.email>", "To Name <to@address.email>", "alsoto@address.email")
            {
                BodyHtml = emailBody,
                Sender = _EmailOptions.FromEmailAddress,
                Subject = subject,
                ApiKey = _EmailOptions.ApiKey,
            };

            string logoPath = Path.Combine(_environment.ContentRootPath, "Assets", "Images", "PRS-Primary-Logo.png");
            message.AddInlineImage(logoPath, "image/png");

            foreach ((byte[] content, string fileName) in attachments)
            {
                string mime = fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? "application/pdf" : "application/octet-stream";
                message.AddAttachment(fileName, Convert.ToBase64String(content), mime);
            }

            foreach (string emailTo in to)
            {
                message.AddToAddress(emailTo);
            }

            EmailResponse res = await _smtp2GoService.SendEmail(message);

            if (res.ResponseStatus == "Forbidden")
                _logger.LogError("Failed to send email");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }

        return true;
    }
}

