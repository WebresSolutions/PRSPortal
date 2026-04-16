using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// Fee proposal / quote for a contact and job type, with lifecycle dates and optional link to a job.
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

    public string QuoteReference { get; set; } = null!;

    public decimal? TotalPrice { get; set; }

    /// <summary>
    /// When the client accepted the proposal (may mirror quote_acceptance.accepted_at).
    /// </summary>
    public DateTime? DateAccepted { get; set; }

    /// <summary>
    /// When the proposal was emailed or otherwise sent to the client.
    /// </summary>
    public DateTime? DateSentToClient { get; set; }

    public DateTime? TargetDeliveryDate { get; set; }

    /// <summary>
    /// Optional last time the client opened the proposal via portal or tracking.
    /// </summary>
    public DateTime? ViewByClientAt { get; set; }

    public string? Description { get; set; }

    public int? QuoteSentByUserId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Soft delete; NULL means active quote.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public virtual Address? Address { get; set; }

    public virtual Contact? Contact { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual Job? Job { get; set; }

    public virtual JobType JobType { get; set; } = null!;

    public virtual AppUser? ModifiedByUser { get; set; }

    public virtual QuoteAcceptance? QuoteAcceptance { get; set; }

    public virtual ICollection<QuoteItem> QuoteItems { get; set; } = new List<QuoteItem>();

    public virtual ICollection<QuoteNote> QuoteNotes { get; set; } = new List<QuoteNote>();

    public virtual AppUser? QuoteSentByUser { get; set; }

    public virtual ICollection<QuoteStatusHistory> QuoteStatusHistories { get; set; } = new List<QuoteStatusHistory>();

    public virtual ICollection<QuoteToken> QuoteTokens { get; set; } = new List<QuoteToken>();

    public virtual QuoteStatus Status { get; set; } = null!;
}
