using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// Track the status history of the job.
/// </summary>
public partial class JobStatusHistory
{
    public int Id { get; set; }

    public int JobId { get; set; }

    public int StatusIdOld { get; set; }

    public int StatusIdNew { get; set; }

    public DateTime DateChanged { get; set; }

    public int ModifiedByUserId { get; set; }

    public virtual Job Job { get; set; } = null!;

    public virtual AppUser ModifiedByUser { get; set; } = null!;

    public virtual JobStatus StatusIdNewNavigation { get; set; } = null!;

    public virtual JobStatus StatusIdOldNavigation { get; set; } = null!;
}
