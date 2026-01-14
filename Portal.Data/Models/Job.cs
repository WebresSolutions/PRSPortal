using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Main job/project tracking with invoicing
/// </summary>
public partial class Job
{
    public int Id { get; set; }

    public int ContactId { get; set; }

    public int? AddressId { get; set; }

    public int? CouncilId { get; set; }

    public int? JobColourId { get; set; }

    public int JobTypeId { get; set; }

    public int? LegacyId { get; set; }

    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Used in junction with the job type to identify the job. With either be type Construction or Surveying
    /// </summary>
    public int? JobNumber { get; set; }

    public string? Details { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Soft delete TIMESTAMPTZ - NULL means active
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public virtual Address? Address { get; set; }

    public virtual Contact Contact { get; set; } = null!;

    public virtual Council? Council { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual JobColour? JobColour { get; set; }

    public virtual ICollection<JobFile> JobFiles { get; set; } = new List<JobFile>();

    public virtual ICollection<JobNote> JobNotes { get; set; } = new List<JobNote>();

    public virtual ICollection<JobQuote> JobQuotes { get; set; } = new List<JobQuote>();

    public virtual ICollection<JobTask> JobTasks { get; set; } = new List<JobTask>();

    public virtual JobType JobType { get; set; } = null!;

    public virtual AppUser? ModifiedByUser { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual ICollection<TimesheetEntry> TimesheetEntries { get; set; } = new List<TimesheetEntry>();

    public virtual ICollection<UserJob> UserJobs { get; set; } = new List<UserJob>();
}
