using NetTopologySuite.Geometries;
using Portal.Data.Models;
using Portal.Shared;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.File;
using Portal.Shared.DTO.Schedule;

namespace Portal.Server.Helpers;

/// <summary>
/// Extension methods for mapping between models and DTOs.
/// </summary>
public static class MapperExtensionMethods
{
    public static Schedule ScheduleToDataObject(this UpdateScheduleDto dto)
    {
        return new Schedule()
        {
            Id = dto.Id,
            JobId = dto.JobId,
            Notes = dto.Notes,
            StartTime = dto.Start,
            EndTime = dto.End,
            ScheduleTrackId = dto.TrackId,
            ScheduleColourId = dto.ColourId,
            DeletedAt = dto.Delete ? DateTime.UtcNow : null
        };
    }


    /// <summary>
    /// Maps a schedule track to a Dto
    /// </summary>
    /// <param name="dataObject"></param>
    /// <returns></returns>
    public static ScheduleTrackDto ScheduleTrackToDto(this ScheduleTrack dataObject)
    {
        return new ScheduleTrackDto()
        {
            AssignedUsers = [.. dataObject.ScheduleUsers?.Select(x => new Shared.DTO.User.UserDto(x.UserId, x.User.DisplayName)) ?? []],
            Schedule = [],
            Day = dataObject.Date ?? DateOnly.MaxValue,
            TrackId = dataObject.Id
        };
    }

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
            FileName = appFile.FileName,
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
            FileName = dto.FileName,
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

    /// <summary>
    /// Maps an AddressDto to an Address data object. Caller must set audit fields such as CreatedByUserId or ModifiedByUserId when persisting.
    /// </summary>
    /// <param name="dto">The address DTO to map.</param>
    /// <returns>The mapped Address data object.</returns>
    public static Address ToAddress(this AddressDto dto, int userId)
    {
        Address address = new()
        {
            Id = dto.AddressId,
            Street = dto.Street,
            PostCode = dto.PostCode,
            Suburb = dto.Suburb,
            StateId = dto.StateId != 0 ? dto.StateId : (int?)dto.State ?? (int)StateEnum.VIC,
            Country = "AUS",
            CreatedByUserId = dto.AddressId is 0 ? userId : dto.CreatedByUserId,
            CreatedOn = dto.AddressId is 0 ? DateTime.UtcNow : dto.CreatedDate
        };

        if (dto.LatLng is not null)
            address.Geom = new Point(new Coordinate(dto.LatLng.Latitude, dto.LatLng.Longitude));

        return address;
    }
}
