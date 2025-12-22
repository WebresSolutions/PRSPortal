using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// Colour of the schedule
/// </summary>
public partial class ScheduleColour
{
    public int Id { get; set; }

    public string Color { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
