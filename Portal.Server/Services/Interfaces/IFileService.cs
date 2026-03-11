using Portal.Data.Models;
using Portal.Shared.DTO.File;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

public interface IFileService
{
    Task<Result<bool>> DeleteFile(int fileId, int modifiedByUserId);
    Task<Result<AppFile>> SaveFile(FileDto file, int modifiedByUserId);
}
