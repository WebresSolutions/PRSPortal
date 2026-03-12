using Portal.Data.Models;
using Portal.Shared.DTO.File;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

public interface IFileService
{
    /// <summary>
    /// Deletes a file
    /// </summary>
    /// <param name="fileId"></param>
    /// <param name="modifiedByUserId"></param>
    /// <returns></returns>
    Task<Result<bool>> DeleteFile(int fileId, int modifiedByUserId);
    /// <summary>
    /// Saves a file 
    /// </summary>
    /// <param name="file"></param>
    /// <param name="modifiedByUserId"></param>
    /// <returns></returns>
    Task<Result<AppFile>> SaveFile(FileDto file, int modifiedByUserId);
    /// <summary>
    /// Gets file data
    /// </summary>
    /// <param name="fileId">The file Id</param>
    /// <returns>A file dto</returns>
    Task<Result<FileDto>> GetFileData(int fileId);
}
