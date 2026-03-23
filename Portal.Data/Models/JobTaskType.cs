using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

public partial class JobTaskType
{
    public int Id { get; set; }

    public int JobTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual JobType JobType { get; set; } = null!;
}
