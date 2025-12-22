using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Instances;

public class ScheduleService(PrsDbContext _prsDbContext, ILogger<ScheduleService> _logger) : IScheduleService
{
    public async Task<Result<List<ScheduleSlotDTO>>> GetScheduleSlotsForDate(DateOnly dateOnly, JobTypeEnum jobType)
    {
        Result<List<ScheduleSlotDTO>> result = new();
        try
        {
            IQueryable<ScheduleSlotDTO> query = _prsDbContext.ScheduleTracks
                .Where(st => st.Date == dateOnly && st.JobTypeId == (int)jobType)
                .AsSplitQuery() // Add this line
                .Select(st => new ScheduleSlotDTO()
                {
                    Day = st.Date ?? dateOnly,
                    AssignedUsers = st.ScheduleUsers.Select(su
                        => new UserDto(su.UserId, su.User.DisplayName)).ToList(),
                    SlotId = st.Id,
                    Schedule = st.Schedules.Select(s
                        => new ScheduleDto()
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
                            ScheduleSlotID = s.ScheduleTrackId,
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

            string queriedSql = query.ToQueryString();

            // Get the schedule tracks 
            List<ScheduleSlotDTO> trackForDate = await query
                .ToListAsync();

            result.Value = trackForDate;


            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all jobs");
            return result.SetError(ErrorType.InternalError, "Failed to get schedule tracks");
        }
    }

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

                  }).ToListAsync();

            result.Value = colours;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all jobs");
            return result.SetError(ErrorType.InternalError, "Failed to get schedule tracks");
        }
    }

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

                if (scheduleColour.Color == colour.ColourHex)

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
}
