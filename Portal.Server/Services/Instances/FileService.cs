using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.File;
using Portal.Shared.Helpers;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Instances;

public class FileService : IFileService
{
    public FileService(ISharePointService sharepointService, PrsDbContext dbContext, ILogger<FileService> logger)
    {
        _sharepointService = sharepointService;
        _dbContext = dbContext;
        _logger = logger;
    }

    private readonly ISharePointService _sharepointService;
    private readonly PrsDbContext _dbContext;
    private readonly ILogger<FileService> _logger;

    /// <summary>
    /// Uploads a file to the sharepoint service and creates/updates an AppFile record in the database. If the file already exists and the content has not changed (determined by comparing file hashes), it will return the existing file without re-uploading.
    /// </summary>
    /// <param name="file">The file being uploaded</param>
    /// <param name="modifiedByUserId"></param>
    /// <returns></returns>
    public async Task<Result<AppFile>> SaveFile(FileDto file, int modifiedByUserId)
    {
        Result<AppFile> result = new();
        try
        {
            // Validate that that file 
            (bool, string?) validationRes = ValidateFile(file);

            if (!validationRes.Item1)
                return result.SetError(ErrorType.BadRequest, validationRes.Item2);

            // validate that the file type exists in the database
            if (await _dbContext.FileTypes.FindAsync(file.FileTypeId) is null)
                return result.SetError(ErrorType.BadRequest, "Invalid file type.");

            // Convert the file to a memory stream for uploading
            using MemoryStream memeStream = new(file.Content);
            // Create the file hash 
            string fileHash = await FileHelper.GetFileHash(file.Content);

            if (file.FileId is not 0)
            {

                if (await _dbContext.AppFiles.FindAsync(file.FileId) is not AppFile existingFile)
                    return result.SetError(ErrorType.NotFound, "File not found.");

                // If the file hash matches, return the existing file without re-uploading
                if (existingFile.FileHash == fileHash)
                    return result.SetValue(existingFile);

                string externalId;

                // Only re-upload the file if the content has changed (determined by comparing file hashes) or if the filename has changed.
                // This prevents unnecessary uploads to the sharepoint service when only the filename is updated.
                if (existingFile.FileHash != fileHash || existingFile.Filename != file.FileName)
                    externalId = string.IsNullOrEmpty(existingFile.ExternalId)
                        ? await _sharepointService.SaveFileAsync(memeStream, file.FileName, file.FileName, []) ?? throw new Exception("Error occured while saving the file to sharepoint")
                        : await _sharepointService.ReplaceDriveItemData(memeStream, existingFile.ExternalId) ?? throw new Exception("Error occured while saving the file to sharepoint");
                else
                    externalId = existingFile.ExternalId ?? throw new Exception("Existing file is missing ExternalId.");

                existingFile.FileHash = fileHash;
                existingFile.ModifiedOn = DateTime.UtcNow;
                existingFile.ExternalId = externalId;
                existingFile.ModifiedByUserId = modifiedByUserId;
                existingFile.Filename = file.FileName;
                existingFile.FileTypeId = file.FileTypeId;

                _dbContext.AppFiles.Update(existingFile);
                await _dbContext.SaveChangesAsync();
                result.SetValue(existingFile);
            }
            else
            {
                // Save the file to the graph service
                string? savedFile = await _sharepointService.SaveFileAsync(memeStream, file.FileName, file.FileName, []);

                if (savedFile == null)
                    return result.SetError(ErrorType.InternalError, "Failed to save file.");

                // Create a new AppFile record in the database
                AppFile newfile = new()
                {
                    Filename = file.FileName,
                    FileHash = fileHash,
                    ExternalId = savedFile,
                    FileTypeId = file.FileTypeId,
                    Description = file.Description,
                    CreatedByUserId = modifiedByUserId,
                    ModifiedByUserId = modifiedByUserId,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                };

                await _dbContext.AppFiles.AddAsync(newfile);
                await _dbContext.SaveChangesAsync();

                result.SetValue(newfile);
            }


            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file to sharepoint");
            throw;
        }
    }

    public async Task<Result<bool>> DeleteFile(int fileId, int modifiedByUserId)
    {
        Result<bool> result = new();
        try
        {
            AppFile? file = await _dbContext.AppFiles.FindAsync(fileId);
            if (file == null)
                return result.SetError(ErrorType.NotFound, "File not found.");

            // TODO: Add delete file function in the sharepoint service or maybe a way to archive the file.
            //_sharepointService.ReplaceFileAndMoveToArchive

            file.DeletedAt = DateTime.UtcNow;
            file.ModifiedByUserId = modifiedByUserId;

            await _dbContext.SaveChangesAsync();
            return result.SetValue(true);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private (bool, string?) ValidateFile(FileDto file)
    {
        // Implement file validation logic here (e.g., check file type, size limits, etc.)
        return (true, null);
    }
}
