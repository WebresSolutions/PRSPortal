using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.DTO.User;

namespace Portal.Shared.DTO.Job;

public class JobDetailsDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the job.
    /// </summary>
    public int JobId { get; set; }
    /// <summary>
    /// Gets or sets the job number associated with this instance.
    /// </summary>
    public int JobNumber { get; set; }
    /// <summary>
    /// Details about the job, such as scope, requirements, or any relevant information that provides context for the job.
    /// </summary>
    public string? Details { get; set; }
    /// <summary>
    /// Gets or sets the type of job to be processed.
    /// </summary>
    public JobTypeEnum JobType { get; set; }
    /// <summary>
    /// Get or set the job colour ID
    /// </summary>
    public int? JobColourId { get; set; }
    /// <summary>
    /// The last modified date of the job
    /// </summary>
    public LastModifiedDto? LastModified { get; set; }
    /// <summary>
    /// Last Modified By User Name
    /// </summary>
    public string? LastModifiedBy { get; set; }
    /// <summary>
    /// The address dto
    /// </summary>
    public AddressDTO? Address { get; set; }
    /// <summary>
    /// Gets or sets the colour information associated with the job.
    /// </summary>
    public JobColourDto? Colour { get; set; }
    /// <summary>
    /// Gets or sets the contact information associated with the job.
    /// </summary>
    public JobContactDto? PrimaryContact { get; set; }
    /// <summary>
    /// Secondary Job Contacts
    /// </summary>
    public List<TechnicalContactDto> TechnicalContacts { get; set; } = [];
    /// <summary>
    /// The Contact Id
    /// </summary>
    public int ContactId { get; set; }
    /// <summary>
    /// Gets or sets the council information associated with the job.
    /// </summary>
    public JobCouncilDto? Council { get; set; }
    /// <summary>
    /// The Council Id
    /// </summary>
    public int? CouncilId { get; set; }
    /// <summary>
    /// Count of notes for the job
    /// </summary>
    public int NoteCount { get; set; }

    /// <summary>
    /// The Task Count
    /// </summary>
    public int TaskCount { get; set; }
    /// <summary>
    /// The Technical Contact Counts
    /// </summary>
    public int ContactCount { get; set; }
    /// <summary>
    /// The Site Visit Count
    /// </summary>
    public int SiteVisitCount { get; set; }
    /// <summary>
    /// Gets or sets the collection of notes associated with the job.
    /// </summary>
    public List<JobNoteDto> Notes { get; set; } = [];
    /// <summary>
    /// Gets or sets the collection of site visits associated with the job.
    /// </summary>
    public List<JobSiteVisitsDto> SiteVisits { get; set; } = [];
    /// <summary>
    /// List of tasks
    /// </summary>
    public List<JobTaskDto> Tasks { get; set; } = [];

    /// <summary>
    /// List of timesheets
    /// </summary>
    public List<TimeSheetDto> TimeSheets { get; set; } = [];
}
