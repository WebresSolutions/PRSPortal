using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// User schedules for work hours.
/// </summary>
public partial class Schedule
{
    public int Id { get; set; }

    public int? LegacyId { get; set; }

    /// <summary>
    /// Start time of the schedule
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End time of the schedule
    /// </summary>
    public DateTime EndTime { get; set; }

    public int? JobId { get; set; }

    public string? Notes { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual Job? Job { get; set; }

    public virtual AppUser? ModifiedByUser { get; set; }
}
