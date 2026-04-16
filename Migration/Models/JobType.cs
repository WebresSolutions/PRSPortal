using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// Type of job
/// </summary>
public partial class JobType
{
    public int Id { get; set; }

    /// <summary>
    /// Construction = Set out. Survey = CAD.
    /// </summary>
    public string Name { get; set; } = null!;

    public string Abbreviation { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public virtual ICollection<FileType> FileTypes { get; set; } = new List<FileType>();

    public virtual ICollection<JobStatus> JobStatuses { get; set; } = new List<JobStatus>();

    public virtual ICollection<JobTaskType> JobTaskTypes { get; set; } = new List<JobTaskType>();

    public virtual ICollection<QuoteTemplate> QuoteTemplates { get; set; } = new List<QuoteTemplate>();

    public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();

    public virtual ICollection<ScheduleTrack> ScheduleTracks { get; set; } = new List<ScheduleTrack>();

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
