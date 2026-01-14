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

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual ICollection<ScheduleTrack> ScheduleTracks { get; set; } = new List<ScheduleTrack>();
}
