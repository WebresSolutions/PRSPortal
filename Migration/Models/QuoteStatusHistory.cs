using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// Track the status history of the quote.
/// </summary>
public partial class QuoteStatusHistory
{
    public int Id { get; set; }

    public int QuoteId { get; set; }

    public int StatusIdOld { get; set; }

    public int StatusIdNew { get; set; }

    public DateTime DateChanged { get; set; }

    public int ModifiedByUserId { get; set; }

    public virtual AppUser ModifiedByUser { get; set; } = null!;

    public virtual Quote Quote { get; set; } = null!;

    public virtual QuoteStatus StatusIdNewNavigation { get; set; } = null!;

    public virtual QuoteStatus StatusIdOldNavigation { get; set; } = null!;
}
