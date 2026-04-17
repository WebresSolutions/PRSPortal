using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Billable or quotable service catalog; referenced by quote_item and quote_template_item.
/// </summary>
public partial class ServiceType
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public string ServiceName { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<QuoteItem> QuoteItems { get; set; } = new List<QuoteItem>();

    public virtual ICollection<QuoteTemplateItem> QuoteTemplateItems { get; set; } = new List<QuoteTemplateItem>();
}
