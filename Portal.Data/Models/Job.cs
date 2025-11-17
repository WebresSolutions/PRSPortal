using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Main job/project tracking with invoicing
/// </summary>
public partial class Job
{
    public int Id { get; set; }

    public int? ContactId { get; set; }

    public int? AddressId { get; set; }

    public int? CouncilId { get; set; }

    public int? JobColourId { get; set; }

    public int? JobTypeId { get; set; }

    public int? LegacyId { get; set; }

    public string? InvoiceNumber { get; set; }

    public int? SurveyNumber { get; set; }

    public int? ConstructionNumber { get; set; }

    /// <summary>
    /// Total job price - consider calculating from timesheet entries
    /// </summary>
    public decimal? TotalPrice { get; set; }

    public DateTime? DateSent { get; set; }

    public DateTime? PaymentReceived { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Soft delete TIMESTAMPTZ - NULL means active
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public virtual Address? Address { get; set; }

    public virtual Contact? Contact { get; set; }

    public virtual Council? Council { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual JobColour? JobColour { get; set; }

    public virtual ICollection<JobFile> JobFiles { get; set; } = new List<JobFile>();

    public virtual ICollection<JobNote> JobNotes { get; set; } = new List<JobNote>();

    public virtual ICollection<JobQuote> JobQuotes { get; set; } = new List<JobQuote>();

    public virtual JobType? JobType { get; set; }

    public virtual AppUser? ModifiedByUser { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual ICollection<TimesheetEntry> TimesheetEntries { get; set; } = new List<TimesheetEntry>();

    public virtual ICollection<UserJob> UserJobs { get; set; } = new List<UserJob>();
}
