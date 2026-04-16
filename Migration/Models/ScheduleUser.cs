using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// Users assigned to work on a given schedule_track (planning roster line).
/// </summary>
public partial class ScheduleUser
{
    public int Id { get; set; }

    public int ScheduleTrackId { get; set; }

    public int UserId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual ScheduleTrack ScheduleTrack { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
