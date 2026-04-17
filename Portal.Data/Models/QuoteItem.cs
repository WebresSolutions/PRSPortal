using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Line items on a quote: service snapshot, price, and optional link to service_type.
/// </summary>
public partial class QuoteItem
{
    public int Id { get; set; }

    public int QuoteId { get; set; }

    public int? ServiceId { get; set; }

    /// <summary>
    /// Service label as shown on the quote at line creation time (denormalised from catalog).
    /// </summary>
    public string ServiceNameSnapshot { get; set; } = null!;

    public decimal Total { get; set; }

    public string? Notes { get; set; }

    public virtual Quote Quote { get; set; } = null!;

    public virtual ServiceType? Service { get; set; }
}
