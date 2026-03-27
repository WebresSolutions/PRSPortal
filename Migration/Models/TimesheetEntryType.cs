using System;
using System.Collections.Generic;

namespace Migration.Models;

public partial class TimesheetEntryType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<TimesheetEntry> TimesheetEntries { get; set; } = new List<TimesheetEntry>();
}
