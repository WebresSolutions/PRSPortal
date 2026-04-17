using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Lookup of how a user is assigned to a job (e.g. lead, member) for job_user.assignment_type_id.
/// </summary>
public partial class JobAssignmentType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<JobUser> JobUsers { get; set; } = new List<JobUser>();
}
