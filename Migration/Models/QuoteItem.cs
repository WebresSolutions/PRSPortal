using System;
using System.Collections.Generic;

namespace Migration.Models;

public partial class QuoteItem
{
    public int Id { get; set; }

    public int QuoteId { get; set; }

    public int? ServiceId { get; set; }

    public string ServiceNameSnapshot { get; set; } = null!;

    public decimal Total { get; set; }

    public string? Notes { get; set; }

    public virtual Quote Quote { get; set; } = null!;

    public virtual ServiceType? Service { get; set; }
}
