using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class ScheduleTrack
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public int Ordinal { get; set; }

    public DateOnly? Date { get; set; }

    public int ScheduleGroupId { get; set; }

    public int? AssigneeUserId1 { get; set; }

    public int? AssigneeUserId2 { get; set; }
}
