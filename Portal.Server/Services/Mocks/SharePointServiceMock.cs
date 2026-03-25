using Microsoft.Graph;
using Microsoft.Graph.Models;
using Moq;
using Portal.Server.Options;
using Portal.Server.Services.Instances;
namespace Portal.Server.Services.Mocks;


/// <summary>
/// A mock service for the sharepoint serivce.
/// </summary>
/// <param name="config"></param>
public class SharePointServiceMock(SharePointOptions config, ILogger logger) : SharePointService(config, logger)
{
    /// <summary>
    /// Override for GetGraphclient
    /// </summary>
    /// <see cref="GraphService.GetGraphClient"/>
    /// <returns>An instance of <see cref="GraphServiceClient"/>.</returns>
    public override GraphServiceClient GetGraphClient()
        => new Mock<GraphServiceClient>().Object;

    /// <summary>
    /// Override for SaveFileAsync
    /// </summary>
    /// <see cref="GraphService.SaveFileAsync"/>
    /// <param name="fileStream">The byte stream for the file.</param>
    /// <param name="subFolder">The subfolder to write to.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="tag">The list of tags to add to the file.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the new external reference.</returns>
    public override Task<string?> SaveFileAsync(Stream fileStream, string subFolder, string fileName, List<string> tag)
        => Task.FromResult<string?>(Guid.NewGuid().ToString());

    /// <summary>
    /// Override for CreateLinkToFile
    /// </summary>
    /// <see cref="GraphService.CreateLinkToFile"/>
    /// <param name="subFolder">The path of the subfolder to which the link will point.</param>
    /// <param name="auditSubFolder">The path of the folder where the link file will be created.</param>
    /// <returns>The ID of the created link file, or null if the operation fails.</returns>
    public override Task<string?> CreateLinkToFile(string subFolder, string auditSubFolder)
        => Task.FromResult<string?>(Guid.NewGuid().ToString());

    /// <summary>
    /// Override for ReplaceDrivedItemData
    /// </summary>
    /// <see cref="GraphService.ReplaceDriveItemData"/>
    /// <param name="fileStream">The stream containing the file data.</param>
    /// <param name="driveItemId">The ID of the drive item to replace.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the uploaded file.</returns>
    public override Task<string?> ReplaceDriveItemData(Stream fileStream, string driveItemId)
        => Task.FromResult<string?>(Guid.NewGuid().ToString());

    /// <summary>
    /// Override for ReplaceAndMoveToArchive
    /// </summary>
    /// <see cref="GraphService.ReplaceFileAndMoveToArchive(Stream, string, string, string)"/>
    /// <param name="newFileDataStream">Stream containing the new file data.</param>
    /// <param name="newFileName">The name of the new file.</param>
    /// <param name="existingId">The ID of the existing file to replace.</param>
    /// <param name="archiveDirectoryPath">The path of the archive directory.</param>
    /// <returns>A tuple containing the new file ID and the archived file ID.</returns>
    public override Task<(string?, string?)> ReplaceFileAndMoveToArchive(
        Stream newFileDataStream,
        string newFileName,
        string existingId,
        string archiveDirectoryPath)
        => Task.FromResult<(string?, string?)>((Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

    /// <summary>
    /// Override for GetLinkToFile
    /// <see cref="GraphService.GetLinkToFile"/>
    /// </summary>
    /// <param name="fileItemId">The Id of the file</param>
    /// <param name="daysUntilExpiry">Days remaining until the sharepoint link expires</param>
    /// <returns>The sharepoint link web url</returns>
    public override Task<string> GetLinkToFile(string? fileItemId, int? daysUntilExpiry, List<string>? recipientEmails = null, bool? blocksDownload = false)
        => Task.FromResult(Guid.NewGuid().ToString());

    /// <summary>
    /// Override which gets a dummy files
    /// </summary>
    /// <param name="externalId">The External Id</param>
    /// <returns>A dummy file</returns>
    public override async Task<(string FileName, string ContentType, byte[] FileBytes)> GetFileByIdAsync(string externalId)
    {
        const string dummyFile = "Screenshot 2025-10-31 094058.png";
        string path = Path.Combine(Directory.GetCurrentDirectory(), $"TestFiles/{dummyFile}");
        byte[] result = await File.ReadAllBytesAsync(path);
        return (dummyFile, "png", result);
    }

    public override async Task<DriveItem?> CreateDirectoryStructure(string directory) => null;
}