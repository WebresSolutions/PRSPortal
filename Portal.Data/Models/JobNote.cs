using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Notes specifically attached to a job (One-to-Many: Job to Note).
/// </summary>
public partial class JobNote
{
    public int Id { get; set; }

    public int JobId { get; set; }

    public string Content { get; set; } = null!;

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual Job Job { get; set; } = null!;

    public virtual AppUser? ModifiedByUser { get; set; }
}
