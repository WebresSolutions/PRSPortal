using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.DTO.File;

/// <summary>
/// Represents a file data transfer object.
/// </summary>
public class FileDto
{
    /// <summary>
    /// Gets or sets the identifier of the related job.
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the file.
    /// </summary>
    public int FileId { get; set; }

    /// <summary>
    /// Gets or sets the content type (MIME type) of the file.
    /// </summary>
    [Required]
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the binary content of the file.
    /// </summary>
    public byte[] Content { get; set; } = [];

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    [Required]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the file.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The title of the file
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Gets or sets the date the file was created.
    /// </summary>
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// Gets or sets the date the file was last modified.
    /// </summary>
    public DateTime DateModified { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the creator of the file.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the string representation of the file type.
    /// </summary>
    public string? FileType { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the file type.
    /// </summary>
    public int FileTypeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file has been deleted.
    /// </summary>
    public bool Deleted { get; set; }
}
