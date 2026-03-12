using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// File metadata and storage references
/// </summary>
public partial class AppFile
{
    public int Id { get; set; }

    public int FileTypeId { get; set; }

    public string FileName { get; set; } = null!;

    public string? Title { get; set; }

    public string? Description { get; set; }

    public long ContentSize { get; set; }

    public string FilePath { get; set; } = null!;

    public string FileExtension { get; set; } = null!;

    /// <summary>
    /// Reference to external storage system (S3, etc)
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// SHA-256 hash for duplicate detection
    /// </summary>
    public string FileHash { get; set; } = null!;

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Soft delete TIMESTAMPTZ - NULL means active
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual FileType FileType { get; set; } = null!;

    public virtual ICollection<JobFile> JobFiles { get; set; } = new List<JobFile>();

    public virtual AppUser? ModifiedByUser { get; set; }
}
