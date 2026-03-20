using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Default lines for a quote_template; copy to quote_item when a template is applied.
/// </summary>
public partial class QuoteTemplateItem
{
    public int Id { get; set; }

    public int QuoteTemplateId { get; set; }

    public int LineOrder { get; set; }

    public int? ServiceId { get; set; }

    public string ServiceNameSnapshot { get; set; } = null!;

    public decimal DefaultRate { get; set; }

    public decimal DefaultQuantity { get; set; }

    public string? Notes { get; set; }

    public virtual QuoteTemplate QuoteTemplate { get; set; } = null!;

    public virtual ServiceType? Service { get; set; }
}
