using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// Many-to-many relationship between users and jobs
/// </summary>
public partial class UserJob
{
    public int UserId { get; set; }

    public int JobId { get; set; }

    public int? LegacyId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Soft delete TIMESTAMPTZ - NULL means active assignment
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual Job Job { get; set; } = null!;

    public virtual AppUser? ModifiedByUser { get; set; }

    public virtual AppUser User { get; set; } = null!;
}
