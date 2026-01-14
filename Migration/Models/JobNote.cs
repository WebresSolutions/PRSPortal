using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// Many-to-many relationship between users and jobs
/// </summary>
public partial class JobNote
{
    public int Id { get; set; }

    public int? AssignedUserId { get; set; }

    public int JobId { get; set; }

    public int? LegacyId { get; set; }

    public string Note { get; set; } = null!;

    public bool ActionRequired { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Soft delete TIMESTAMPTZ - NULL means to show
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public virtual AppUser? AssignedUser { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual Job Job { get; set; } = null!;

    public virtual AppUser? ModifiedByUser { get; set; }
}
