using System;
using System.Collections.Generic;

namespace Migration.Models;

public partial class JobAssignmentType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<JobUser> JobUsers { get; set; } = new List<JobUser>();
}
