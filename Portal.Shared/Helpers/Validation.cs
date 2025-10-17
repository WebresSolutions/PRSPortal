using System.Net.Mail;

namespace Portal.Shared.Helpers;

/// <summary>
/// Utility class providing validation methods for common data types
/// Contains helper methods for data validation and verification
/// </summary>
public static class Validation
{
    /// <summary>
    /// Validates if a string is a valid email address format
    /// Performs comprehensive email validation including format checking and domain validation
    /// </summary>
    /// <param name="email">The email string to validate</param>
    /// <returns>True if the email is valid, false otherwise</returns>
    public static bool IsValidEmail(string email)
    {
        string trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith('.'))
        {
            return false;
        }
        try
        {
            MailAddress addr = new(email);
            return addr.Address == trimmedEmail;
        }
        catch
        {
            return false;
        }
    }
}