using Microsoft.AspNetCore.Components;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Types;

namespace Portal.Client.Components.JobComponents;

public partial class JobStatusProgress
{
    /// <summary>
    /// Status definitions for the pipeline (typically from <see cref="JobDetailsDto.JobTypeStatusDtos"/>).
    /// </summary>
    [Parameter]
    public IReadOnlyList<JobTypeStatusDto>? Statuses { get; set; }

    /// <summary>
    /// The job's current status id, or null if unset / unknown.
    /// </summary>
    [Parameter]
    public int? CurrentStatusId { get; set; }

    private IReadOnlyList<JobTypeStatusDto> OrderedStatuses =>
        Statuses?.OrderBy(s => s.Sequence).ToList() ?? [];

    private int CurrentStatusIndex
    {
        get
        {
            if (CurrentStatusId is null)
                return -1;
            IReadOnlyList<JobTypeStatusDto> list = OrderedStatuses;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == CurrentStatusId)
                    return i;
            }

            return -1;
        }
    }

    private string ProgressState(int stepIndex)
    {
        int current = CurrentStatusIndex;
        if (current < 0)
            return "upcoming";
        if (stepIndex < current)
            return "completed";
        if (stepIndex == current)
            return "current";
        return "upcoming";
    }
}
