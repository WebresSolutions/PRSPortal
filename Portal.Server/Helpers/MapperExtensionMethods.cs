using Portal.Data.Models;
using Portal.Shared.DTO.File;

namespace Portal.Server.Helpers;

/// <summary>
/// Extension methods for mapping between models and DTOs.
/// </summary>
public static class MapperExtensionMethods
{
    /// <summary>
    /// Maps an AppFile to a FileDto.
    /// </summary>
    /// <param name="appFile">The AppFile to map.</param>
    /// <returns>The mapped FileDto.</returns>
    public static FileDto ToDto(this AppFile appFile)
    {
        return new FileDto
        {
            FileId = appFile.Id,
            FileName = appFile.Filename,
            Description = appFile.Description,
            Title = appFile.Title ?? "",
            DateCreated = appFile.CreatedOn,
            DateModified = appFile.ModifiedOn,
            CreatedBy = appFile.CreatedByUser?.DisplayName,
            FileType = appFile.FileType?.Name,
            FileTypeId = appFile.FileTypeId,
            Deleted = appFile.DeletedAt.HasValue,
            JobId = 0,
            ContentType = "",
            Content = []
        };
    }

    /// <summary>
    /// Maps a FileDto to an AppFile. Caller must set ExternalId, FileHash, CreatedByUserId and ModifiedByUserId when persisting.
    /// </summary>
    /// <param name="dto">The FileDto to map.</param>
    /// <returns>The mapped AppFile (Id, user ids, ExternalId and FileHash are not set from DTO).</returns>
    public static AppFile ToAppFile(this FileDto dto)
    {
        return new AppFile
        {
            Id = dto.FileId,
            FileTypeId = dto.FileTypeId,
            Filename = dto.FileName,
            Title = string.IsNullOrEmpty(dto.Title) ? null : dto.Title,
            Description = dto.Description,
            CreatedOn = dto.DateCreated,
            ModifiedOn = dto.DateModified,
            DeletedAt = dto.Deleted ? DateTime.UtcNow : null,
            FileHash = null!,   // Caller must set after computing hash
            CreatedByUserId = 0, // Caller must set
            ModifiedByUserId = 0  // Caller must set
        };
    }
}
