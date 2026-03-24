using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.DTO.Types;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Instances;

/// <summary>
/// Service implementation for schedule-related business logic
/// Handles schedule slot retrieval, color management, and data transformation
/// </summary>
public class ScheduleService(PrsDbContext _prsDbContext, ILogger<ScheduleService> _logger) : IScheduleService
{
    /// <summary>
    /// Retrieves schedule slots for a specific date and job type
    /// </summary>
    /// <param name="dateOnly">The date to retrieve schedule slots for</param>
    /// <param name="jobType">The job type to filter by</param>
    /// <returns>A result containing a list of schedule slot DTOs</returns>
    public async Task<Result<List<ScheduleTrackDto>>> GetScheduleSlotsForDate(HttpContext context, DateOnly dateOnly, JobTypeEnum jobType)
    {
        Result<List<ScheduleTrackDto>> result = new();
        try
        {
            IQueryable<ScheduleTrackDto> query = _prsDbContext.ScheduleTracks
                .Where(st => st.Date == dateOnly && st.JobTypeId == (int)jobType && st.DeletedAt == null)
                .AsSplitQuery() // Add this line
                .Select(st => new ScheduleTrackDto()
                {
                    Day = st.Date ?? dateOnly,
                    AssignedUsers = st.ScheduleUsers.Select(su
                        => new UserDto(su.UserId, su.User.DisplayName)).ToList(),
                    TrackId = st.Id,
                    Schedule = st.Schedules.Select(s
                        => new ScheduleDto()
                        {
                            ScheduleId = s.Id,
                            ScheduleTrackDate = st.Date!.Value,
                            Start = s.StartTime,
                            End = s.EndTime,
                            Description = s.Notes ?? "",
                            Colour = new ScheduleColourDto()
                            {
                                ScheduleColourId = s.ScheduleColour.Id,
                                Description = s.ScheduleColour.Color,
                                ColourHex = s.ScheduleColour.Color
                            },
                            ScheduleTrackId = s.ScheduleTrackId,
                            Job = s.Job != null ? new ScheduleJobPartialDto()
                            {
                                JobId = s.Job.Id,
                                JobNumber = s.Job.JobNumber,
                                Address = s.Job.AddressId != null ?
                                    new AddressDTO(s.Job.AddressId.Value,
                                                   (StateEnum)s.Job.Address!.StateId!,
                                                   s.Job.Address.StateId ?? 0,
                                                   s.Job.Address.Suburb,
                                                   s.Job.Address.Street,
                                                   s.Job.Address.PostCode)
                                    : null
                            } : null
                        }).ToList()
                });

            // Get the schedule tracks 
            List<ScheduleTrackDto> trackForDate = await query
                .ToListAsync();

            List<ScheduleTrack> newTracks = [];
            if (trackForDate.Count == 0)
            {
                for (int i = 0; i < 4; i++)
                    newTracks.Add(new ScheduleTrack()
                    {
                        CreatedByUserId = context.UserId(), // System User
                        CreatedOn = DateTime.UtcNow,
                        Date = dateOnly,
                        JobTypeId = (int)jobType,
                    });
                await _prsDbContext.ScheduleTracks.AddRangeAsync(newTracks);
                await _prsDbContext.SaveChangesAsync();
                trackForDate = await query.ToListAsync();
            }

            result.Value = trackForDate;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get schedules");
            return result.SetError(ErrorType.InternalError, "Failed to get schedule tracks");
        }
    }

    /// <summary>
    /// Gets a single schedule by id.
    /// </summary>
    /// <param name="id">The schedule id.</param>
    /// <returns>The schedule DTO if found.</returns>
    public async Task<Result<ScheduleDto>> GetSchedule(int id)
    {
        Result<ScheduleDto> result = new();
        try
        {
            Schedule? schedule = await _prsDbContext.Schedules
                .AsNoTracking()
                .Include(s => s.ScheduleColour)
                .Include(s => s.ScheduleTrack)
                .Include(s => s.Job).ThenInclude(j => j!.Address)
                .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null);

            if (schedule is null)
            {
                return result.SetError(ErrorType.NotFound, $"Schedule with Id {id} not found.");
            }

            DateOnly trackDate = schedule.ScheduleTrack?.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);
            result.Value = new ScheduleDto
            {
                ScheduleId = schedule.Id,
                Start = schedule.StartTime,
                End = schedule.EndTime,
                ScheduleTrackId = schedule.ScheduleTrackId,
                ScheduleTrackDate = trackDate,
                Colour = new ScheduleColourDto
                {
                    ScheduleColourId = schedule.ScheduleColour.Id,
                    ColourHex = schedule.ScheduleColour.Color,
                    Description = schedule.ScheduleColour.Color
                },
                Description = schedule.Notes ?? "",
                Job = schedule.Job != null
                    ? new ScheduleJobPartialDto
                    {
                        JobId = schedule.Job.Id,
                        JobNumber = schedule.Job.JobNumber,
                        Address = schedule.Job.AddressId != null && schedule.Job.Address != null
                            ? new AddressDTO(
                                schedule.Job.AddressId.Value,
                                (StateEnum)(schedule.Job.Address.StateId ?? 0),
                                schedule.Job.Address.StateId ?? 0,
                                schedule.Job.Address.Suburb ?? "",
                                schedule.Job.Address.Street ?? "",
                                schedule.Job.Address.PostCode ?? "")
                            : null
                    }
                    : null
            };
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get schedule {ScheduleId}", id);
            return result.SetError(ErrorType.InternalError, "Failed to load schedule.");
        }
    }

    /// <summary>
    /// Updates or creates a schedule
    /// </summary>
    /// <param name="context">The http contex</param>
    /// <param name="data">The dto being saved</param>
    /// <returns></returns>
    public async Task<Result<int>> UpdateSchedule(HttpContext context, UpdateScheduleDto data)
    {
        Result<int> result = new();
        try
        {
            if (data.Start > data.End)
                return result.SetError(ErrorType.BadRequest, "The start date must be after the end date.");

            if ((data.End - data.Start).Hours > 12)
                return result.SetError(ErrorType.BadRequest, "The time cannot span for longer than 12 hourss.");

            if (await _prsDbContext.ScheduleTracks.FindAsync(data.TrackId) is not ScheduleTrack track)
                return result.SetError(ErrorType.BadRequest, $"Invalid Schedule Track Id: {data.TrackId}");

            if (data.JobId is not null && await _prsDbContext.Jobs.FindAsync(data.JobId) is null)
                return result.SetError(ErrorType.BadRequest, $"Invalid Schedule Job Id: {data.TrackId}");

            if (await _prsDbContext.ScheduleColours.FindAsync(data.ColourId) is null)
                return result.SetError(ErrorType.BadRequest, $"Invalid Schedule colour Id: {data.TrackId}");

            // Create the new schedule 
            int userId = context.UserId();

            if (data.Id is 0)
            {
                Schedule newSchedule = data.ScheduleToDataObject();
                newSchedule.CreatedByUserId = userId;
                newSchedule.DeletedAt = null;
                await _prsDbContext.Schedules.AddAsync(newSchedule);
                await _prsDbContext.SaveChangesAsync();
                return result.SetValue(newSchedule.Id);
            }
            else
            {
                if (await _prsDbContext.Schedules.FindAsync(data.Id) is not Schedule existing)
                    return result.SetError(ErrorType.BadRequest, $"Invalid Schedule Id: {data.Id}");

                existing.ModifiedByUserId = userId;
                existing.ModifiedOn = DateTime.UtcNow;
                // Store as UTC kind so PostgreSQL accepts it, but do not convert - 7am stays 7am for everyone
                existing.StartTime = data.Start;
                existing.EndTime = data.End;
                existing.DeletedAt = data.Delete ? DateTime.UtcNow : null;
                existing.Notes = data.Notes;
                existing.ScheduleColourId = data.ColourId;
                existing.JobId = data.JobId;

                await _prsDbContext.SaveChangesAsync();
                return result.SetValue(existing.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create new schedule");
            return result.SetError(ErrorType.InternalError, "Failed to create new schedule");
        }
    }

    /// <summary>
    /// Creates a schedule slot for a date. Schedules are assigned on a separate endpoint
    /// </summary>
    /// <param name="date">The date the slot is created</param>
    /// <returns>The created schedule slot as a DTO</returns>
    public async Task<Result<ScheduleTrackDto>> UpdateScheduleTrack(HttpContext context, UpdateScheduleTrackDto data)
    {
        Result<ScheduleTrackDto> result = new();
        try
        {
            int userId = context.UserId();

            if (data.ScheduleTrackId is 0)
            {
                ScheduleTrack newTrack = new()
                {
                    CreatedByUserId = context.UserId(), // System User
                    CreatedOn = DateTime.UtcNow,
                    Date = data.Date,
                    JobTypeId = (int)data.JobTypeEnum
                };

                await _prsDbContext.ScheduleTracks.AddAsync(newTrack);
                await _prsDbContext.SaveChangesAsync();

                IEnumerable<ScheduleUser> usersToAdd = data.AssignedUsers.Select(x => new ScheduleUser() { CreatedByUserId = userId, UserId = x, ScheduleTrackId = newTrack.Id, CreatedOn = DateTime.UtcNow });
                await _prsDbContext.ScheduleUsers.AddRangeAsync(usersToAdd);
                await _prsDbContext.SaveChangesAsync();

                newTrack = await _prsDbContext.ScheduleTracks.Include(x => x.ScheduleUsers).ThenInclude(su => su.User).FirstOrDefaultAsync(x => x.Id == newTrack.Id)
                    ?? throw new Exception("Failed to find the newly created schedule track");

                return result.SetValue(newTrack.ScheduleTrackToDto());
            }
            else
            {
                if (await _prsDbContext.ScheduleTracks.Include(x => x.ScheduleUsers).FirstOrDefaultAsync(x => x.Id == data.ScheduleTrackId) is not ScheduleTrack track)
                    return result.SetError(ErrorType.BadRequest, $"Schedule track with Id: {data.ScheduleTrackId} does not exist");

                List<AppUser> assignedUsers = await _prsDbContext.AppUsers.Where(x => data.AssignedUsers.Contains(x.Id)).ToListAsync();
                // Ensure that the users are valid
                if (data.AssignedUsers.Count != assignedUsers.Count)
                    return result.SetError(ErrorType.BadRequest, "Invalid Schedule User Id provided");

                track.Date = data.Date;
                track.ModifiedByUserId = userId;
                track.ModifiedOn = DateTime.UtcNow;
                track.JobTypeId = (int)data.JobTypeEnum;

                // Remove the users from the schedules
                _ = await _prsDbContext.ScheduleUsers.Where(x => x.ScheduleTrackId == data.ScheduleTrackId).ExecuteDeleteAsync();
                IEnumerable<ScheduleUser> usersToAdd = data.AssignedUsers.Select(x => new ScheduleUser() { CreatedByUserId = userId, UserId = x, ScheduleTrackId = track.Id, CreatedOn = DateTime.UtcNow });
                await _prsDbContext.ScheduleUsers.AddRangeAsync(usersToAdd);
                await _prsDbContext.SaveChangesAsync();

                ScheduleTrack? updatedTrack = await _prsDbContext.ScheduleTracks
                    .Include(x => x.ScheduleUsers)
                    .ThenInclude(su => su.User)
                    .FirstOrDefaultAsync(x => x.Id == track.Id);
                return updatedTrack != null
                    ? result.SetValue(updatedTrack.ScheduleTrackToDto())
                    : result.SetError(ErrorType.InternalError, "Failed to load updated schedule track");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create schedule slot.");
            return result.SetError(ErrorType.InternalError, "Failed to create the schedule slot.");
        }
    }

    /// <summary>
    /// Soft delete a schedule track
    /// </summary>
    /// <param name="context">The http context of the caller </param>
    /// <param name="id">The id of the track to delete</param>
    /// <returns>The Id of the track.</returns>
    public async Task<Result<int>> DeleteTrack(HttpContext context, int id)
    {
        Result<int> res = new();
        try
        {
            // find the schedule track
            if (await _prsDbContext.ScheduleTracks.FindAsync(id) is not ScheduleTrack track)
                return res.SetError(ErrorType.BadRequest, $"Invalid schedule track Id: {id}");

            track.DeletedAt = DateTime.UtcNow;
            track.ModifiedByUserId = context.UserId();
            track.ModifiedByUser = null;
            await _prsDbContext.SaveChangesAsync();
            return res.SetValue(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create schedule slot.");
            return res.SetError(ErrorType.InternalError, "Failed to create the schedule slot.");
        }
    }

    /// <summary>
    /// Gets a schedule for the week
    /// </summary>
    /// <param name="jobType">Filtered by job type </param>
    /// <param name="weekDay">The day of the week requested</param>
    /// <returns>A result containing the weekly schedule as an array</returns>
    public async Task<Result<WeeklyGroupedByScheduleDto[]>> GetWeeklySchedule(JobTypeEnum jobType, DateOnly? weekDay)
    {
        Result<WeeklyGroupedByScheduleDto[]> result = new();
        try
        {
            // If null set the date to now
            weekDay ??= DateOnly.FromDateTime(DateTime.UtcNow);
            DateOnly monday = GetMondayFromDate(weekDay!.Value);
            DateOnly endOfWeek = monday.AddDays(7);

            WeeklyScheduleDto[] schedules = await _prsDbContext.Schedules
                .AsSplitQuery()
                .Where(x =>
                    x.DeletedAt == null
                    && x.ScheduleTrack != null
                    && x.ScheduleTrack.Date >= monday
                    && x.ScheduleTrack.Date < endOfWeek
                    && x.ScheduleTrack.DeletedAt == null
                    && x.ScheduleTrack.JobTypeId == (int)jobType)
                .Select(s =>
                    new WeeklyScheduleDto()
                    {
                        TrackDate = s.ScheduleTrack.Date!.Value,
                        AssignedUsers = s.ScheduleTrack.ScheduleUsers
                            .Select(su => new UserDto(su.UserId, su.User.DisplayName))
                            .ToArray(),
                        ScheduleTrackId = s.ScheduleTrackId,
                        Schedule = new ScheduleDto()
                        {
                            ScheduleId = s.Id,
                            Start = s.StartTime,
                            End = s.EndTime,
                            Description = s.Notes ?? "",
                            Colour = new ScheduleColourDto()
                            {
                                ScheduleColourId = s.ScheduleColour.Id,
                                Description = s.ScheduleColour.Color,
                                ColourHex = s.ScheduleColour.Color
                            },
                            ScheduleTrackId = s.ScheduleTrackId,
                            Job = s.Job != null ? new ScheduleJobPartialDto()
                            {
                                JobId = s.Job.Id,
                                JobNumber = s.Job.JobNumber,
                                Address = s.Job.AddressId != null ?
                                            new AddressDTO(s.Job.AddressId.Value,
                                                           (StateEnum)s.Job.Address!.StateId!,
                                                           s.Job.Address.StateId ?? 0,
                                                           s.Job.Address.Suburb,
                                                           s.Job.Address.Street,
                                                           s.Job.Address.PostCode)
                                            : null
                            } : null
                        }
                    })
                .ToArrayAsync();

            WeeklyGroupedByScheduleDto[] groupedByDay = [.. schedules
                .GroupBy(x => x.TrackDate)
                .Select(y => new WeeklyGroupedByScheduleDto()
                {
                    Schedules = [.. y],
                    Date = y.Key,
                    DayOfWeek = y.Key.DayOfWeek
                }
                ).OrderBy(x => x.Date)];

            result.Value = groupedByDay;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get weekly schedule");
            return result.SetError(ErrorType.InternalError, "Failed to get weekly schedule");
        }
    }

    /// <summary>
    /// Retrieves all available schedule colors
    /// </summary>
    /// <returns>A result containing a list of schedule color DTOs</returns>
    public async Task<Result<List<ScheduleColourDto>>> GetScheduleColours()
    {
        Result<List<ScheduleColourDto>> result = new();
        try
        {
            List<ScheduleColourDto> colours = await _prsDbContext.ScheduleColours
                  .Select(sc => new ScheduleColourDto()
                  {
                      ScheduleColourId = sc.Id,
                      ColourHex = sc.Color,

                  })
                  .OrderBy(sc => sc.ScheduleColourId)
                  .ToListAsync();

            result.Value = colours;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all jobs");
            return result.SetError(ErrorType.InternalError, "Failed to get schedule tracks");
        }
    }

    /// <summary>
    /// Updates an existing schedule color or creates a new one if the ID is 0
    /// </summary>
    /// <param name="colour">The schedule color DTO containing the color information</param>
    /// <returns>A result containing the updated or created schedule color DTO</returns>
    public async Task<Result<ScheduleColourDto>> UpdateScheduleColour(ScheduleColourDto colour)
    {
        Result<ScheduleColourDto> result = new();
        try
        {
            ScheduleColour? scheduleColour;
            // Adding new colour
            if (colour.ScheduleColourId is 0)
            {
                scheduleColour = new ScheduleColour
                {
                    Color = colour.ColourHex
                };
                await _prsDbContext.ScheduleColours.AddAsync(scheduleColour);
                await _prsDbContext.SaveChangesAsync();
            }
            else
            {
                scheduleColour = await _prsDbContext.ScheduleColours
                    .FirstOrDefaultAsync(sc => sc.Id == colour.ScheduleColourId);

                if (scheduleColour is null)
                    return result.SetError(ErrorType.BadRequest, "Schedule colour not found");

                scheduleColour.Color = colour.ColourHex;

                await _prsDbContext.SaveChangesAsync();
            }
            result.Value = new ScheduleColourDto { ScheduleColourId = scheduleColour.Id, ColourHex = scheduleColour.Color };
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update schedule colour");
            return result.SetError(ErrorType.InternalError, "Failed to update schedule colour");
        }
    }

    /// <summary>
    /// Calculates the monday from the provided date
    /// </summary>
    /// <param name="date">The date supplied</param>
    /// <returns>The date with the previous monday</returns>
    private static DateOnly GetMondayFromDate(DateOnly date)
    {
        if (date.DayOfWeek is DayOfWeek.Monday)
            return date;

        int dayOfWeek = (int)date.DayOfWeek;
        if (dayOfWeek < (int)DayOfWeek.Monday)
            return date.AddDays(1);

        int difference = (int)date.DayOfWeek - dayOfWeek;
        return date.AddDays(-difference);
    }

}
