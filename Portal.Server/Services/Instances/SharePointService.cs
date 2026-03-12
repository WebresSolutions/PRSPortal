using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Items.Item;
using Microsoft.Graph.Drives.Item.Items.Item.Checkin;
using Microsoft.Graph.Drives.Item.Items.Item.CreateLink;
using Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions;
using Moq;
using Portal.Server.Options;
using Portal.Server.Services.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace Portal.Server.Services.Instances;

/// <summary>
/// Service class for interacting with Microsoft Graph API.
/// Provides methods for working with SharePoint sites, lists, files, and users.
/// </summary>
public class SharePointService : ISharePointService
{
    /// <summary>
    /// Configurable options for sharepoint
    /// </summary>
    private readonly SharePointOptions _options;
    /// <summary>
    /// The client secret credential used to authenticate with Azure services.
    /// </summary>
    private ClientSecretCredential? ClientSecretCredential { get; set; }

    /// <summary>
    /// Constructor that takes a config
    /// </summary>
    /// <param name="config"></param>
    public SharePointService(SharePointOptions config)
    {
        _options = config;
        CreateClientCredential();
    }

    /// <summary>
    /// Creates the client credentials required to connect to Azure services.
    /// </summary>
    private void CreateClientCredential()
    {
        try
        {
            // Set up the token credential options
            TokenCredentialOptions options = new()
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };
            // Create a new ClientSecretCredential
            if (_options.UseMock)
            {
                ClientSecretCredential = new Mock<ClientSecretCredential>().Object;
                return;
            }
            ClientSecretCredential = new ClientSecretCredential(_options.TenantId, _options.ClientId, _options.ClientSecret, options);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Gets the Microsoft Graph client used to interact with Microsoft services.
    /// </summary>
    /// <returns>An instance of <see cref="GraphServiceClient"/>.</returns>
    public virtual GraphServiceClient GetGraphClient()
    {
        try
        {
            // Ensure that the client credential is initialized
            if (ClientSecretCredential == null)
                throw new InvalidOperationException("ClientSecretCredential is not initialized.");

            // Return a new GraphServiceClient instance using the client secret credentials and scope
            return new GraphServiceClient(ClientSecretCredential, [_options.Scopes]);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Gets an access token for the service.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the access token string.</returns>
    // ReSharper disable once UnusedMember.Global
    public async Task<string> GetAccessToken()
    {
        string accessToken;

        try
        {
            // Build the authority URL for the tenant
            string authority = $"https://login.microsoftonline.com/{_options.TenantId}";

            // Create a confidential client application
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(_options.ClientId)
                .WithClientSecret(_options.ClientSecret)
                .WithAuthority(new Uri(authority))
                .Build();

            // Acquire an access token for the client
            AuthenticationResult result = await app.AcquireTokenForClient([_options.Scopes])
                .ExecuteAsync();

            // Set the access token
            accessToken = result.AccessToken;
        }
        catch (Exception)
        {
            throw;
        }
        return accessToken;
    }

    #region SharePoint Sites

    /// <summary>
    /// Gets the site ID for a given site URL.
    /// </summary>
    /// <param name="graphClient">The graph client instance.</param>
    /// <param name="siteUrl">The URL of the site.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the site ID as a string.</returns>
    public async Task<string> GetSiteId(GraphServiceClient graphClient, string siteUrl)
    {
        string siteId = string.Empty;

        try
        {
            // Get a list of SharePoint sites
            List<Site> sites = await GetSharePointSites(graphClient);

            if (sites.Count == 0)
            {
                Console.WriteLine("No sites found.");
                return siteId;
            }
            // Find the site by its URL
            Site? site = sites.FirstOrDefault(dat => dat.WebUrl == siteUrl);

            if (site == null)
                Console.WriteLine($"Site {siteUrl} not found.");
            else
                siteId = site.Id ?? string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get site collections. Error: {ex.Message}");
        }
        return siteId;
    }

    /// <summary>
    /// Gets a list of SharePoint sites.
    /// </summary>
    /// <param name="graphClient">The graph client instance (optional).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of SharePoint sites.</returns>
    public async Task<List<Site>> GetSharePointSites(GraphServiceClient? graphClient = null)
    {
        List<Site> siteCollections = [];

        try
        {
            // Ensure that the graph client is initialized
            graphClient ??= GetGraphClient();

            // Get a collection of sites
            SiteCollectionResponse? sites = await graphClient.Sites.GetAsync();

            if (sites?.Value != null)
                siteCollections = [.. sites.Value];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get site collections. Error: {ex.Message}");
        }
        return siteCollections;
    }

    /// <summary>
    /// Gets a SharePoint site by its name.
    /// </summary>
    /// <param name="graphClient">The graph client instance (optional).</param>
    /// <param name="siteName">The name of the site.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a SharePoint site.</returns>
    public async Task<Site?> GetSharePointSite(GraphServiceClient? graphClient = null, string siteName = "")
    {
        try
        {
            // Ensure that the graph client is initialized
            graphClient ??= GetGraphClient();

            if (string.IsNullOrWhiteSpace(siteName))
                return null;

            // Get the site by its name
            return await graphClient.Sites[siteName].GetAsync();
        }
        catch (ServiceException ex)
        {
            Console.WriteLine($"Failed to get site. Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets a list of SharePoint drives for a site.
    /// </summary>
    /// <param name="graphClient">The graph client instance (optional).</param>
    /// <param name="siteId">The ID of the site.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of SharePoint drives.</returns>
    public async Task<List<Drive>> GetSharePointDrives(GraphServiceClient? graphClient = null, string siteId = "")
    {
        List<Drive> drives = [];

        try
        {
            // Ensure that the graph client is initialized
            graphClient ??= GetGraphClient();

            if (string.IsNullOrWhiteSpace(siteId))
                return drives;

            // Get a collection of drives for the site
            DriveCollectionResponse? drivesList = await graphClient.Sites[siteId].Drives.GetAsync();

            if (drivesList?.Value != null)
            {
                drives = [.. drivesList.Value];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get site drives for site id {siteId}. Error: {ex.Message}");
            throw;
        }
        return drives;
    }

    #endregion

    /// <summary>
    /// Gets a file by its external ID from SharePoint.
    /// </summary>
    /// <param name="externalId">The external ID for the file.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the file name, content type, and file bytes.</returns>
    public virtual async Task<(string FileName, string ContentType, byte[] FileBytes)> GetFileByIdAsync(string externalId)
    {
        try
        {
            // Initialize Graph client and mime type map
            GraphServiceClient graphClient = GetGraphClient();

            // Get the drive for the site
            Drive driveItem = await graphClient.Sites[_options.SiteId].Drive.GetAsync() ?? throw new Exception($"SharePoint site not found - {_options.SiteId}");

            // Build the request for the file
            DriveItemItemRequestBuilder itemRequestBuilder = graphClient.Drives[driveItem.Id].Items[externalId];
            ListItem sharepointFile = await itemRequestBuilder.ListItem.GetAsync() ?? throw new Exception($"File not found - {externalId}");

            // Get the file name and content type
            string? fileName = Path.GetFileName(sharepointFile.WebUrl);

            if (fileName == null)
                return ("", "", []);

            byte[] buffer = [];
            // Get the file stream
            Stream? fileStream = await itemRequestBuilder.Content.GetAsync();

            if (fileStream == null)
            {
                return (fileName, "", buffer);
            }
            using MemoryStream ms = new();
            await fileStream.CopyToAsync(ms);
            buffer = ms.ToArray();
            return (fileName, "", buffer);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Saves a file to SharePoint.
    /// </summary>
    /// <param name="fileStream">The byte stream for the file.</param>
    /// <param name="subFolder">The subfolder to write to.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="tag">The list of tags to add to the file.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the new external reference.</returns>
    public virtual async Task<string?> SaveFileAsync(Stream fileStream, string subFolder, string fileName, List<string> tag)
    {
        try
        {
            // Initialize Graph client
            GraphServiceClient graphClient = GetGraphClient();

            // Get the drive based on the site id
            Drive drive = await graphClient.Sites[_options.SiteId].Drive.GetAsync()
                          ?? throw new Exception($"SharePoint site not found - {_options.SiteId}");
            // The Id of the drive being uploaded to
            string driveId = drive.Id ?? string.Empty;

            // Get the Id of the folder to upload to
            DriveItem uploadFolder = await GetSubFolders(graphClient, driveId, subFolder)
                                     ?? throw new Exception($"Failed to get/create the upload folder for - {subFolder}");

            // Upload the new file. 
            DriveItem uploadResult = await UploadFile(
                graphClient,
                fileStream,
                driveId,
                fileName,
                uploadFolder.Id ?? throw new Exception($"Failed to upload the file - {subFolder}/{fileName}"),
                false);
            // Return the result Id
            return uploadResult.Id;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Creates a shortcut link file in a SharePoint folder. This method uploads a `.url` file that points to a specified subfolder using Microsoft Graph API.
    /// </summary>
    /// <param name="subFolder">The path of the subfolder to which the link will point.</param>
    /// <param name="auditSubFolder">The path of the folder where the link file will be created.</param>
    /// <returns>The ID of the created link file, or null if the operation fails.</returns>
    public virtual async Task<string?> CreateLinkToFile(string subFolder, string auditSubFolder)
    {
        try
        {
            // Initialize Graph client
            GraphServiceClient graphClient = GetGraphClient();

            // Get the drive based on the site ID
            Drive drive = await graphClient.Sites[_options.SiteId].Drive.GetAsync() ?? throw new Exception($"SharePoint site not found - {_options.SiteId}");
            // The ID of the drive being uploaded to
            string driveId = drive.Id ?? string.Empty;

            // The folder which is being linked to.
            DriveItem linkFolder = await GetSubFolders(graphClient, driveId, subFolder)
                                   ?? throw new Exception($"Failed to get/create the folder for - {subFolder}");

            // The folder which the link is being added to.
            DriveItem uploadFolder = await GetSubFolders(graphClient, driveId, auditSubFolder)
                                     ?? throw new Exception($"Failed to get/create the folder for - {auditSubFolder}");
            // Construct the link name ending with .url
            string linkName = $"{GetSafeFolderName(auditSubFolder.Split('/').Last())}.url";
            // Create the content for the .url file
            byte[] fileContent = Encoding.UTF8.GetBytes($"[InternetShortcut]\nURL={linkFolder.WebUrl}");

            // Upload the content as a new file in the target directory
            using MemoryStream stream = new(fileContent);

            DriveItem linkFile = await graphClient.Drives[driveId].Items[uploadFolder.Id]
                .ItemWithPath(linkName)
                .Content
                .PutAsync(stream) ?? throw new Exception("Failed To Create Sharepoint link");
            // Check in the file
            CheckinPostRequestBody checkInRequest = new() { Comment = "Init link Check In" };
            await graphClient.Drives[driveId].Items[linkFile.Id].Checkin.PostAsync(checkInRequest);
            // Return the link drive item ID
            return linkFile.Id;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Replaces the data of an existing drive item in SharePoint and creates a new sharepoint file version.
    /// </summary>
    /// <param name="fileStream">The stream containing the file data.</param>
    /// <param name="driveItemId">The ID of the drive item to replace.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the uploaded file.</returns>
    public virtual async Task<string?> ReplaceDriveItemData(Stream fileStream, string driveItemId)
    {
        try
        {
            // Initialize Graph client
            GraphServiceClient graphClient = GetGraphClient();

            // Get the drive based on the site ID
            Drive drive = await graphClient.Sites[_options.SiteId].Drive.GetAsync()
                          ?? throw new Exception($"SharePoint site not found - {_options.SiteId}");
            string driveId = drive.Id ?? string.Empty;

            // Get the drive item to be replaced
            DriveItem currentDriveItem = await graphClient.Drives[driveId].Items[driveItemId].GetAsync()
                                         ?? throw new Exception($"Drive item not found ID - {driveItemId}");

            // Get the id of the parent directory
            string directoryId = currentDriveItem.ParentReference?.Id
                                 ?? throw new Exception($"Drive item not found ID - {currentDriveItem.ParentReference?.Id}");

            // If the file is checked out by another user, attempt to check it in
            await graphClient.Drives[driveId].Items[driveItemId].Checkout.PostAsync();

            DriveItem uploadResult = await UploadFile(
                graphClient,
                fileStream,
                driveId,
                currentDriveItem.Name ?? throw new Exception("Invalid File Name"),
                directoryId);

            // Return the ID of the uploaded file
            return uploadResult.Id;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Replaces an existing file in SharePoint and moves the old file to an archive folder.
    /// </summary>
    /// <param name="newFileDataStream">Stream containing the new file data.</param>
    /// <param name="newFileName">The name of the new file.</param>
    /// <param name="existingId">The ID of the existing file to replace.</param>
    /// <param name="archiveDirectoryPath">The path of the archive directory.</param>
    /// <returns>A tuple containing the new file ID and the archived file ID.</returns>
    public virtual async Task<(string?, string?)> ReplaceFileAndMoveToArchive(
        Stream newFileDataStream,
        string newFileName,
        string existingId,
        string archiveDirectoryPath)
    {
        try
        {
            // Get the graph client
            GraphServiceClient graphClient = GetGraphClient();

            // Get the drive based on the site ID
            Drive drive = await graphClient.Sites[_options.SiteId].Drive.GetAsync()
                          ?? throw new Exception($"SharePoint site not found - {_options.SiteId}");
            string driveId = drive.Id ?? string.Empty;

            // Get the drive item to be replaced
            DriveItem currentDriveItem = await graphClient.Drives[driveId].Items[existingId].GetAsync()
                                         ?? throw new Exception($"Drive item not found ID - {existingId}");

            string parentDirectoryId = currentDriveItem.ParentReference?.Id
                                       ?? throw new Exception("Could not find the directory Id to upload the file to.");

            // Get or create the archive folder 
            DriveItem archiveFolder = await GetSubFolders(graphClient, driveId, archiveDirectoryPath)
                                      ?? throw new Exception("Failed to get the archive folder");

            DriveItem moveItemRequestBody = new()
            {
                ParentReference = new ItemReference
                {
                    Id = archiveFolder.Id
                },
                Name = currentDriveItem.Name
            };

            // Move the item. 
            DriveItem moveItemDriveResult = await graphClient.Drives[driveId].Items[currentDriveItem.Id].PatchAsync(moveItemRequestBody)
                                            ?? throw new Exception("Failed to move the drive item to archive.");

            // Upload the new file. 
            DriveItem uploadResult = await UploadFile(
                graphClient,
                newFileDataStream,
                driveId,
                newFileName,
                parentDirectoryId);

            // return the results 
            return (uploadResult.Id, moveItemDriveResult.Id);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Creates a link to a sharepoint file
    /// </summary>
    /// <param name="fileItemId">The Id of the file</param>
    /// <param name="daysUntilExpiry">Days remaining until the sharepoint link expires</param>
    /// <returns>The sharepoint link web url</returns>
    public virtual async Task<string> GetLinkToFile(string? fileItemId, int? daysUntilExpiry, List<string>? recipientEmails = null, bool? blocksDownload = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileItemId))
                return "";

            GraphServiceClient graphClient = GetGraphClient();

            // Get the drive based on the site ID
            Drive drive = await graphClient.Sites[_options.SiteId].Drive.GetAsync()
                          ?? throw new Exception($"SharePoint site not found - {_options.SiteId}");
            string driveId = drive.Id ?? string.Empty;

            // Create the request body
            CreateLinkPostRequestBody requestBody = new()
            {
                Type = blocksDownload == true ? "blocksDownload" : "view",
                ExpirationDateTime = daysUntilExpiry is not null ? DateTime.UtcNow.AddDays(daysUntilExpiry.Value) : null,
                Scope = "anonymous",
                RetainInheritedPermissions = false
            };

            // If there are recipients, add them to the request body. 
            if (recipientEmails is { Count: > 0 })
            {
                requestBody.Recipients = [.. recipientEmails.Select(email => new DriveRecipient { Email = email })];
            }

            // Create the file link
            Permission? result = await graphClient.Drives[driveId].Items[fileItemId].CreateLink.PostAsync(requestBody);

            // Null check on the result and the link
            if (result is not { Link.WebUrl: { } webUrl })
                throw new Exception("Failed to create the SharePoint link");

            // Return the web url
            return webUrl;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Uploads a file to a specific folder in SharePoint.
    /// </summary>
    /// <param name="graphClient">The Graph service client for SharePoint interactions.</param>
    /// <param name="fileDataStream">Stream containing the file data.</param>
    /// <param name="driveId">The ID of the drive where the file is stored.</param>
    /// <param name="fileName">The name of the file to upload.</param>
    /// <param name="folderToUploadToId">The ID of the folder where the file will be uploaded.</param>
    /// <returns>The uploaded drive item.</returns>
    private async Task<DriveItem> UploadFile(
        GraphServiceClient graphClient,
        Stream fileDataStream,
        string driveId,
        string fileName,
        string folderToUploadToId,
        bool replace = true)
    {
        try
        {

            // Specify the file conflict behaviour
            CreateUploadSessionPostRequestBody uploadSessionRequestBody = new()
            {
                Item = new DriveItemUploadableProperties
                {
                    AdditionalData = new Dictionary<string, object> { { "@microsoft.graph.conflictBehavior", replace ? "replace" : "rename" } }
                }
            };

            // Any file over 2MB must be uploaded with an upload session
            UploadSession? uploadSession = await graphClient.Drives[driveId]
                .Items[folderToUploadToId]
                .ItemWithPath(fileName)
                .CreateUploadSession
                .PostAsync(uploadSessionRequestBody);

            // Max slice size must be a multiple of 320 KiB
            const int maxSliceSize = 320 * 1024;

            // Create the upload task
            LargeFileUploadTask<DriveItem> fileUploadTask = new(uploadSession, fileDataStream, maxSliceSize, graphClient.RequestAdapter);
            UploadResult<DriveItem> uploadResult;

            // Upload the file
            try
            {
                uploadResult = await fileUploadTask.UploadAsync();
            }
            catch (ODataError)
            {
                throw;
            }

            if (uploadResult is null or { ItemResponse: null })
                throw new Exception("Failed to upload the file to sharepoint");

            // By default, files are checked out and not visible so need to check in the file to upload it correctly.
            CheckinPostRequestBody requestBody = new() { Comment = "Init File Check In" };
            await graphClient.Drives[driveId].Items[uploadResult.ItemResponse.Id].Checkin.PostAsync(requestBody);

            return uploadResult.ItemResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }


    /// <summary>
    /// Creates folders in SharePoint based on the specified path using a batch request.
    /// </summary>
    /// <param name="graphClient">The GraphServiceClient instance.</param>
    /// <param name="driveId">The ID of the SharePoint drive.</param>
    /// <param name="directory">The path of subfolders to create.</param>
    /// <returns>The <see cref="DriveItem" upload folder/></returns>
    private async Task<DriveItem?> GetSubFolders(GraphServiceClient graphClient, string driveId, string directory)
    {
        BatchRequestContentCollection batchRequestContent;

        try
        {
            // Get safe list of folders
            string[] safeFolderList = directory.Split('/')
                .Select(GetSafeFolderName)
                .ToArray();

            // Join them back together
            directory = "/" + string.Join("/", safeFolderList);

            // Check if the folder path already exists.
            try
            {
                DriveItem? existingItem = await graphClient.Drives[driveId].Root.ItemWithPath(directory).GetAsync();

                if (existingItem != null)
                {
                    return existingItem;
                }
            }
            finally
            {
                // If the folder doesn't exist, we'll create it with the batch request below. 
            }

            string previousPath = "//";
            string currentPath = string.Empty;
            int lastRequestId = 0;

            // Create the batch request
            batchRequestContent = new BatchRequestContentCollection(graphClient);

            // Add folders to the request
            foreach (string subFolderName in safeFolderList)
            {
                currentPath += $"/{subFolderName}";

                DriveItem folder = new()
                {
                    Name = subFolderName,
                    Folder = new Folder(),
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "@microsoft.graph.conflictBehavior", "replace" }
                    }
                };

                // Use the request builder to generate a request to create a folder
                RequestInformation createFolderRequest = graphClient.Drives[driveId].Root.ItemWithPath(previousPath).Children
                    .ToPostRequestInformation(folder);

                HttpRequestMessage? eventsRequestMessage = await graphClient.RequestAdapter
                    .ConvertToNativeRequestAsync<HttpRequestMessage>(createFolderRequest);

                // Increment the lastRequestId for a unique integer
                int eventsRequestId = ++lastRequestId;
                // Dependency list
                List<int> dependency = lastRequestId > 1 ? [lastRequestId - 1] : [];

                // Add the batch request step
                batchRequestContent.AddBatchRequestStep(new BatchRequestStep(
                    eventsRequestId.ToString(),
                    eventsRequestMessage,
                    dependency.Select(id => id.ToString()).ToList())); // Convert int to string for dependencies

                // Update the last request ID
                lastRequestId = eventsRequestId;
                // Update the previous path for the next iteration
                previousPath = currentPath;
            }
        }
        catch (Exception)
        {
            throw;
        }

        // Execute the batch request
        try
        {
            _ = await graphClient.Batch.PostAsync(batchRequestContent);
        }
        catch (Exception)
        {
            throw;
        }
        // Return the newly created folder
        return await graphClient.Drives[driveId].Root.ItemWithPath(directory).GetAsync();
    }

    /// <summary>
    /// Cleans up a folder name to remove invalid characters.
    /// </summary>
    /// <param name="folderName">The raw folder name.</param>
    /// <returns>The cleaned folder name.</returns>
    private static string GetSafeFolderName(string folderName)
    {
        string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()) + new string(".");
        Regex regex = new($"[{Regex.Escape(invalidChars)}]");
        return regex.Replace(folderName, string.Empty);
    }
}