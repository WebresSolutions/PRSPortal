using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// Colour of job
/// </summary>
public partial class JobColour
{
    public int Id { get; set; }

    public string Color { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
