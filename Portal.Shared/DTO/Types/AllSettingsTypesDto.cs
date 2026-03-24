using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Job;

namespace Portal.Shared.DTO.Types;

/// <summary>All lookup lists used by the Settings page in one response.</summary>
public sealed class AllSettingsTypesDto
{
    public TimeTypeDto[] TimesheetTypes { get; set; } = [];
    public ContactTypeDto[] ContactTypes { get; set; } = [];
    public JobTypeDto[] JobTypes { get; set; } = [];
    public JobColourDto[] JobColours { get; set; } = [];
    public FileTypeDto[] FileTypes { get; set; } = [];
    public JobTaskTypeDto[] JobTaskTypes { get; set; } = [];
    public TechnicalContactTypeDto[] TechnicalContactTypes { get; set; } = [];
    public StateDto[] States { get; set; } = [];
    public ScheduleColourDto[] ScheduleColours { get; set; } = [];
    public ServiceTypeDto[] ServiceTypes { get; set; } = [];
}
