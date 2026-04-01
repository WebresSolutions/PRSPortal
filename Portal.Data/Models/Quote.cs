using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// A quote for a job
/// </summary>
public partial class Quote
{
    public int Id { get; set; }

    public int? LegacyId { get; set; }

    public int StatusId { get; set; }

    public int? AddressId { get; set; }

    public int? ContactId { get; set; }

    public int JobTypeId { get; set; }

    public int? JobId { get; set; }

    public string? QuoteReference { get; set; }

    public decimal? TotalPrice { get; set; }

    public DateTime? DateAccepted { get; set; }

    public DateTime? DateSentToClient { get; set; }

    public DateTime? TargetDeliveryDate { get; set; }

    public string? Description { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Address? Address { get; set; }

    public virtual Contact? Contact { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual Job? Job { get; set; }

    public virtual ICollection<JobQuote> JobQuotes { get; set; } = new List<JobQuote>();

    public virtual JobType JobType { get; set; } = null!;

    public virtual AppUser? ModifiedByUser { get; set; }

    public virtual ICollection<QuoteItem> QuoteItems { get; set; } = new List<QuoteItem>();

    public virtual ICollection<QuoteNote> QuoteNotes { get; set; } = new List<QuoteNote>();

    public virtual ICollection<QuoteStatusHistory> QuoteStatusHistories { get; set; } = new List<QuoteStatusHistory>();

    public virtual QuoteStatus Status { get; set; } = null!;
}
