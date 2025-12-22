using Portal.Shared;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Schedule;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Services.Interfaces;

public interface IApiService
{
    Task<Result<PagedResponse<ListJobDto>>> GetAllJobs(int pageSize, int pageNumber, string? nameFilter, string? orderby, SortDirectionEnum order);
    Task<Result<List<ScheduleSlotDTO>>> GetIndividualSchedule(DateOnly date, JobTypeEnum jobType);
    Task<Result<List<ScheduleColourDto>>> GetScheduleColours();
    Task<Result<ScheduleColourDto>> UpdateScheduleColour(ScheduleColourDto colour);

}
