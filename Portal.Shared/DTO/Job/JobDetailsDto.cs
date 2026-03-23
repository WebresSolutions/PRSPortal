using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.File;
using Portal.Shared.DTO.TimeSheet;
using Portal.Shared.DTO.User;
using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.DTO.Job;

public class JobDetailsDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the job.
    /// </summary>
    public int JobId { get; set; }
    /// <summary>
    /// The status of the jobs
    /// </summary>
    public int? JobStatusId { get; set; }
    public string? JobStatusName { get; set; }
    /// <summary>
    /// Gets or sets the job number associated with this instance.
    /// </summary>
    [Required(ErrorMessage = "Job number is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Job number must be at least 1")]
    public string? JobNumber { get; set; }
    /// <summary>
    /// Details about the job, such as scope, requirements, or any relevant information that provides context for the job.
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Details cannot exceed 2000 characters")]
    public string? Details { get; set; }
    /// <summary>
    /// Gets or sets the type of job to be processed.
    /// </summary>
    [Required(ErrorMessage = "Job type is required")]
    public JobTypeEnum[] JobType { get; set; } = [];
    /// <summary>
    /// Get or set the job colour ID
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Job colour ID must be at least 1 when provided")]
    public int? JobColourId { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    public DateTime DateCreated { get; set; }
    /// <summary>
    /// Last Modified Date
    /// </summary>
    public DateTime? DateModified { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    public string CreatedBy { get; set; } = "";
    /// <summary>
    /// The last modified date of the job
    /// </summary>
    public LastModifiedDto? LastModified { get; set; }
    /// <summary>
    /// Last Modified By User Name
    /// </summary>
    public string? LastModifiedBy { get; set; }
    /// <summary>
    /// The Contact Id
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Contact is required")]
    public int ContactId { get; set; }
    /// <summary>
    /// Gets or sets the council information associated with the job.
    /// </summary>
    public JobCouncilDto? Council { get; set; }
    /// <summary>
    /// The Council Id
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Council ID must be positive when provided")]
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
    /// List of jobs files. Will not included the file content, just metadata such as file name, size, type, etc.
    /// </summary>
    public List<FileDto> JobFiles { get; set; } = [];
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
    /// <summary>
    /// Secondary Job Contacts
    /// </summary>
    public List<TechnicalContactDto> TechnicalContacts { get; set; } = [];

    public List<JobTypeStatusDto> JobTypeStatusDtos { get; set; } = [];
}
