using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Colour of the schedule
/// </summary>
public partial class ScheduleColour
{
    public int Id { get; set; }

    public string Color { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
