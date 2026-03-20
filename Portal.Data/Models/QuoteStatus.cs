using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Holds the status of a quote
/// </summary>
public partial class QuoteStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<QuoteStatusHistory> QuoteStatusHistoryStatusIdNewNavigations { get; set; } = new List<QuoteStatusHistory>();

    public virtual ICollection<QuoteStatusHistory> QuoteStatusHistoryStatusIdOldNavigations { get; set; } = new List<QuoteStatusHistory>();

    public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();
}
