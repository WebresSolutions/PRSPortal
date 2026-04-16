using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// Per job_type catalog of task kinds or labels used when categorising job_task rows.
/// </summary>
public partial class JobTaskType
{
    public int Id { get; set; }

    public int JobTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedOn { get; set; }

    public bool IsActive { get; set; }

    public virtual JobType JobType { get; set; } = null!;
}
