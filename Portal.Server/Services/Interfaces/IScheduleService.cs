using Portal.Shared;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

public interface IScheduleService
{
    Task<Result<List<ScheduleSlotDTO>>> GetScheduleSlotsForDate(DateOnly date, JobTypeEnum jobType);
}
