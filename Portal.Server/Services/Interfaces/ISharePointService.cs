using Microsoft.Graph;
namespace Portal.Server.Services.Interfaces;

/// <summary>
/// Interface for interacting with Microsoft Graph API, particularly for SharePoint operations.
/// Provides methods for working with files, sites, drives, lists, permissions, and users within a SharePoint environment.
/// </summary>
public interface ISharePointService
{
    /// <summary>
    /// Gets or sets the ID of the SharePoint site.
    /// This is used to identify the specific site within SharePoint where operations will be performed.
    /// </summary>
    string SiteId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the drive associated with the SharePoint site.
    /// The drive ID represents the specific document library or drive within the SharePoint site.
    /// </summary>
    string DriveId { get; set; }

    /// <summary>
    /// Gets or sets the subpath or key for the file within the SharePoint site.
    /// This key is used to locate a specific file within the SharePoint structure.
    /// </summary>
    string FileUrlKey { get; set; }

    /// <summary>
    /// Gets or sets the base folder path within the SharePoint site.
    /// This is the root folder where operations such as file uploads and downloads will be performed.
    /// </summary>
    public string BaseFolder { get; set; }

    /// <summary>
    /// Gets or sets the cache folder path used for local caching of SharePoint files.
    /// This folder is used to temporarily store files locally before or after they are processed by the service.
    /// </summary>
    public string CacheFolder { get; set; }

    /// <summary>
    /// Retrieves an instance of <see cref="GraphServiceClient"/> to interact with Microsoft Graph API.
    /// This client is used to perform various operations such as accessing sites, lists, and files within SharePoint.
    /// </summary>
    /// <returns>An instance of <see cref="GraphServiceClient"/> configured for the current service.</returns>
    GraphServiceClient GetGraphClient();

    /// <summary>
    /// Retrieves a file from SharePoint by its external ID.
    /// </summary>
    /// <param name="externalId">The external ID of the file within SharePoint.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the file name, content type, and file bytes.</returns>
    Task<(string FileName, string ContentType, byte[] FileBytes)> GetFileByIdAsync(string externalId);

    /// <summary>
    /// Saves a file to SharePoint.
    /// </summary>
    /// <param name="fileStream">The stream containing the file data.</param>
    /// <param name="subFolderName">The subfolder within SharePoint where the file will be saved.</param>
    /// <param name="fileName">The name of the file to be saved.</param>
    /// <param name="tag">A list of tags to associate with the file in SharePoint.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the new external reference ID for the saved file.</returns>
    Task<string?> SaveFileAsync(Stream fileStream, string subFolderName, string fileName, List<string> tag);

    /// <summary>
    /// Creates a shortcut link file in a SharePoint folder. This method uploads a `.url` file that points to a specified subfolder using Microsoft Graph API.
    /// </summary>
    /// <param name="subFolder">The path of the subfolder to which the link will point.</param>
    /// <param name="auditSubFolder">The path of the folder where the link file will be created.</param>
    /// <returns>The ID of the created link file, or null if the operation fails.</returns>
    Task<string?> CreateLinkToFile(string subFolder, string auditSubFolder);

    /// <summary>
    /// Replaces the data of an existing drive item in SharePoint and creates a new sharepoint file version.
    /// </summary>
    /// <param name="fileStream">The stream containing the file data.</param>
    /// <param name="driveItemId">The ID of the drive item to replace.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the uploaded file.</returns>
    Task<string?> ReplaceDriveItemData(Stream fileStream, string driveItemId);

    /// <summary>
    /// Replaces an existing file in SharePoint and moves the old file to an archive folder.
    /// </summary>
    /// <param name="newFileDataStream">Stream containing the new file data.</param>
    /// <param name="newFileName">The name of the new file.</param>
    /// <param name="existingId">The ID of the existing file to replace.</param>
    /// <param name="archiveDirectoryPath">The path of the archive directory.</param>
    /// <returns>A tuple containing the new file ID and the archived file ID.</returns>
    Task<(string?, string?)> ReplaceFileAndMoveToArchive(Stream newFileDataStream, string newFileName, string existingId, string archiveDirectoryPath);

    /// <summary>
    /// Creates a link to a sharepoint file
    /// </summary>
    /// <param name="fileItemId">The Id of the file</param>
    /// <param name="daysUntilExpiry">Days remaining until the sharepoint link expires</param>
    /// <returns>The sharepoint link web url</returns>
    Task<string> GetLinkToFile(string fileItemId, int? daysUntilExpiry, List<string>? recipientEmails = null, bool? blocksDownload = false);

}
