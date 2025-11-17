using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

public partial class ScheduleTrack
{
    public int Id { get; set; }

    public int JobTypeId { get; set; }

    public int? LegacyId { get; set; }

    public DateOnly? Date { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual JobType JobType { get; set; } = null!;

    public virtual AppUser? ModifiedByUser { get; set; }

    public virtual ICollection<ScheduleUser> ScheduleUsers { get; set; } = new List<ScheduleUser>();
}
