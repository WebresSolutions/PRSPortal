using System;
using System.Collections.Generic;

namespace Migration.Models;

public partial class ServiceType
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public string ServiceName { get; set; } = null!;

    public decimal? DefaultRate { get; set; }

    public string? UnitOfMeasure { get; set; }

    public bool IsActive { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<QuoteItem> QuoteItems { get; set; } = new List<QuoteItem>();

    public virtual ICollection<QuoteTemplateItem> QuoteTemplateItems { get; set; } = new List<QuoteTemplateItem>();
}
