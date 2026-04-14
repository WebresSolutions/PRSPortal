using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Main job/project tracking with invoicing
/// </summary>
public partial class Job
{
    public int Id { get; set; }

    public int? StatusId { get; set; }

    public int ContactId { get; set; }

    public int? AddressId { get; set; }

    public int? CouncilId { get; set; }

    public int? JobColourId { get; set; }

    public int? LegacyId { get; set; }

    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Used in junction with the job type to identify the job. With either be type Construction or Surveying
    /// </summary>
    public string JobNumber { get; set; } = null!;

    public DateTime? TargetDeliveryDate { get; set; }

    public DateTime? LatestClientUpdate { get; set; }

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

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual JobColour? JobColour { get; set; }

    public virtual ICollection<JobFile> JobFiles { get; set; } = new List<JobFile>();

    public virtual ICollection<JobNote> JobNotes { get; set; } = new List<JobNote>();

    public virtual ICollection<JobStatusHistory> JobStatusHistories { get; set; } = new List<JobStatusHistory>();

    public virtual ICollection<JobTask> JobTasks { get; set; } = new List<JobTask>();

    public virtual ICollection<JobUser> JobUsers { get; set; } = new List<JobUser>();

    public virtual AppUser? ModifiedByUser { get; set; }

    public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual JobStatus? Status { get; set; }

    public virtual ICollection<TechnicalContact> TechnicalContacts { get; set; } = new List<TechnicalContact>();

    public virtual ICollection<TimesheetEntry> TimesheetEntries { get; set; } = new List<TimesheetEntry>();

    public virtual ICollection<JobType> JobTypes { get; set; } = new List<JobType>();
}
