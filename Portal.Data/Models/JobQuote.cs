using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Links quotes to jobs
/// </summary>
public partial class JobQuote
{
    public int Id { get; set; }

    public int JobId { get; set; }

    public int? LegacyId { get; set; }

    public int QuoteId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual Job Job { get; set; } = null!;

    public virtual Quote Quote { get; set; } = null!;
}
