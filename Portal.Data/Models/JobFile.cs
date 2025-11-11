using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Files attached to specific jobs
/// </summary>
public partial class JobFile
{
    public int JobId { get; set; }

    public int FileId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual AppFile File { get; set; } = null!;

    public virtual Job Job { get; set; } = null!;
}
