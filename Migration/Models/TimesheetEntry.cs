using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// Time tracking entries for jobs and their associated note content.
/// </summary>
public partial class TimesheetEntry
{
    public int Id { get; set; }

    public int? JobId { get; set; }

    public int UserId { get; set; }

    public string? Description { get; set; }

    public DateTime DateFrom { get; set; }

    /// <summary>
    /// NULL indicates ongoing/in-progress entry
    /// </summary>
    public DateTime? DateTo { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual Job? Job { get; set; }

    public virtual AppUser? ModifiedByUser { get; set; }

    public virtual AppUser User { get; set; } = null!;
}
