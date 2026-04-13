namespace Portal.Client.Services.Interfaces;

/// <summary>Loads the signed-in user's profile photo from Microsoft Graph (Entra ID).</summary>
public interface IEntraProfilePhotoService
{
    /// <summary>Returns a data URI suitable for an <c>img</c> <c>src</c>, or <c>null</c> if none or on failure.</summary>
    Task<string?> GetProfilePhotoDataUriAsync(CancellationToken cancellationToken = default);
}
