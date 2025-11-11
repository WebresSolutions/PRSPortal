using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// A quote for a job
/// </summary>
public partial class Quote
{
    public int Id { get; set; }

    public int? AddressId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Address? Address { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual ICollection<JobQuote> JobQuotes { get; set; } = new List<JobQuote>();

    public virtual AppUser? ModifiedByUser { get; set; }

    public virtual ICollection<QuoteNote> QuoteNotes { get; set; } = new List<QuoteNote>();
}
