using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

public partial class QuoteItem
{
    public int Id { get; set; }

    public int QuoteId { get; set; }

    public int? ServiceId { get; set; }

    public string ServiceNameSnapshot { get; set; } = null!;

    public decimal AppliedRate { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? Subtotal { get; set; }

    public string? Notes { get; set; }

    public virtual Quote Quote { get; set; } = null!;

    public virtual ServiceType? Service { get; set; }
}
