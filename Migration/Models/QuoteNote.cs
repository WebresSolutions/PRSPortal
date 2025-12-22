using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// Notes specifically attached to a quote (One-to-Many: Quote to Note).
/// </summary>
public partial class QuoteNote
{
    public int Id { get; set; }

    public int QuoteId { get; set; }

    public string Content { get; set; } = null!;

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual AppUser? ModifiedByUser { get; set; }

    public virtual Quote Quote { get; set; } = null!;
}
