using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// File type and metadata
/// </summary>
public partial class FileType
{
    public int Id { get; set; }

    public int JobTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedOn { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<AppFile> AppFiles { get; set; } = new List<AppFile>();

    public virtual JobType JobType { get; set; } = null!;
}
