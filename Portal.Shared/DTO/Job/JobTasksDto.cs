namespace Portal.Shared.DTO.Job;

/// <summary>
/// Data transfer object representing a job task
/// Contains task information including description, dates, and invoice requirements
/// </summary>
public class JobTaskDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the task
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the job this task belongs to
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    /// Gets or sets the description or name of the task
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an invoice is required for this task
    /// </summary>
    public bool InvoiceRequired { get; set; }

    /// <summary>
    /// Gets or sets the date when the task becomes active
    /// </summary>
    public DateTime ActiveDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the task was completed
    /// </summary>
    public DateTime CompletedDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the task was invoiced
    /// </summary>
    public DateTime InvoicedDate { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who created this task
    /// </summary>
    public string CreatedByUser { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the task was created
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who last modified this task, if any
    /// </summary>
    public int? ModifiedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the task was last modified, if any
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the task was deleted, if any
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// The quotes price for the task, if available
    /// </summary>
    public Decimal? QuotedPrice { get; set; }
}
