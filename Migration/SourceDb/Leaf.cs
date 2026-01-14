using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class Leaf
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CreatedUser { get; set; }

    public DateTime Created { get; set; }

    public int ModifiedUser { get; set; }

    public DateTime Modified { get; set; }

    public string Type { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateOnly LeaveStartDate { get; set; }

    public DateOnly? LeaveEndDate { get; set; }

    public int Deleted { get; set; }

    public int LeaveStartTimeHour { get; set; }

    public int LeaveEndTimeHour { get; set; }

    public int LeaveStartTimeMinute { get; set; }

    public int LeaveEndTimeMinute { get; set; }
}
