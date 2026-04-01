using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// The status of a Job
/// </summary>
public partial class JobStatus
{
    public int Id { get; set; }

    public int Sequence { get; set; }

    public int JobTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string Colour { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<JobStatusHistory> JobStatusHistoryStatusIdNewNavigations { get; set; } = new List<JobStatusHistory>();

    public virtual ICollection<JobStatusHistory> JobStatusHistoryStatusIdOldNavigations { get; set; } = new List<JobStatusHistory>();

    public virtual JobType JobType { get; set; } = null!;

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
