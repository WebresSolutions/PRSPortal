using Microsoft.Extensions.Options;
using Portal.Server.Options;
using Portal.Server.Services.Interfaces.UtilityServices;
using Smtp2Go.Api;
using Smtp2Go.Api.Models.Emails;

namespace Portal.Server.Services.Instances.Utilities;

public class EmailService(IOptions<EmailOptions> _IEmailOptions, IApiService _smtp2GoService, ILogger<EmailService> _logger) : IEmailService
{
    EmailOptions _EmailOptions = _IEmailOptions.Value;

    ///<inheritdoc  />
    public async Task<bool> SendEmail(string[] to, string subject, string body, (byte[] content, string fileName)[] attachments)
    {
        try
        {
            if (!string.IsNullOrEmpty(_EmailOptions.ToEmailAddressOverride))
                to = [_EmailOptions.ToEmailAddressOverride];

            EmailResponse res = await _smtp2GoService.SendEmail(emailBody, subject, _EmailOptions.FromEmailAddress, to);

            if (res.ResponseStatus == "Forbidden")
                _logger.LogError("Failed to send email");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }

        return true;
    }


    string emailBody = @"
    <!DOCTYPE html>
    <html lang=""en"">
    <head>
      <meta charset=""UTF-8"">
      <title>Contact Form</title>
    </head>
    <body>

    <h2>Contact us</h2>

      <label for=""name"">Name:</label>
      <input type=""text"" id=""name"" name=""name"" required>
      <br>
 
      <label for=""email"">Email Address:</label>
      <input type=""email"" id=""email"" name=""email"" required>
      <br>
 
      <label for=""message"">Message:</label>
      <textarea id=""message"" name=""message"" required></textarea>
      <br>
 
      <button type=""submit"">Send</button>
    </form>

    </body>
    </html>
    ";

}

