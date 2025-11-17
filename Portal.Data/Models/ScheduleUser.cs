using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

public partial class ScheduleUser
{
    public int Id { get; set; }

    public int ScheduleTrackId { get; set; }

    public int? LegacyId { get; set; }

    public int UserId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual ScheduleTrack ScheduleTrack { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
