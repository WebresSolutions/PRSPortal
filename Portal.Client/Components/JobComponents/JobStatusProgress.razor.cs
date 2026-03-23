using Microsoft.AspNetCore.Components;
using Portal.Shared.DTO.Job;

namespace Portal.Client.Components.JobComponents;

public partial class JobStatusProgress
{
    private static readonly string[] StatusStepAccentColors =
    [
        "#1e3a5f", "#0ea5e9", "#0d9488", "#ea580c", "#dc2626", "#7c3aed", "#b45309"
    ];

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
        Statuses?.OrderBy(s => s.order).ToList() ?? [];

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

    private static string StatusStepAccent(int index) =>
        StatusStepAccentColors[index % StatusStepAccentColors.Length];

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
