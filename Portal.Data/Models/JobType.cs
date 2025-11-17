using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Type of job
/// </summary>
public partial class JobType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Abbreviation { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual ICollection<ScheduleTrack> ScheduleTracks { get; set; } = new List<ScheduleTrack>();
}
